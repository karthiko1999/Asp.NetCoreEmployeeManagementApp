using EmployeeManagementApp.Repositories;
using Microsoft.Extensions.DependencyInjection;
using EmployeeManagementApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using EmployeeManagementApp.Models;
using EmployeeManagementApp.Security;
using EmployeeManagementApp.Security.Tokenlifespan;

namespace EmployeeManagement
{
    public class MyStartup
    {
        public IConfiguration _config;

        public MyStartup(IConfiguration config)
        {
            _config = config;
        }
        // This is the Method that get called by runtime,here we are register all our services to the container.
        // Its is called Before Configure() so that we can register our Custom services & use in configure()
        public void ConfigureServices(IServiceCollection services)
        {
            // Environment Set-up for MVC
            services.AddMvc(option => option.EnableEndpointRouting = false);

            // Apply Authorize attribute globally to all our Controllers of our Application.
            // First we need to Build an Authorization policy where it requires users  to Authenticate 
            // Next configurating that Authorization policy as our Authorization filter and the Authorizationfilter as your application filter.
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddViewOptions(options =>
            {
                // This is to perform client side validation
                options.HtmlHelperOptions.ClientValidationEnabled = true;
            }); 

            // Register the application services with different lifetime
            // services.Add(new ServiceDescriptor(typeof(IEmployeeRepository),typeof(MockEmployeeRepository),               ServiceLifetime.Singleton)); 
            //  Here we want the instance EmpRep to be alive and available for the entire scope of the given HTTP request. For another new HTTP request, a new instance
            services.AddScoped<IEmployeeRepository, MySqlEmployeeRepository>();

            // configure the dbcontext to IOC containers
            services.AddDbContextPool<AppDbContext>(options =>
            {
                string connectionString = _config.GetConnectionString("EmployeeDB");

                // DbContextOptionsBuilder to select and configure the DataBase.
                // Serverversion Autodetect() method bascially open and exectes cmd on the Server,Uses a connection string to open a connection to the database server and then executes a command. The connection will ignore the database specified in the connection string. It therefore makes not difference, whether the database already exists or not. 
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });


            // Register Identity services with IOC 
            // #1.IdentityUser - is used by default by the ASP.NET Core Identity framework to manage registered users of your application.  If you want store additional information about the registered users like their Gender, City etc. Create a custom class that derives from IdentityUser. and replace with IdentityUser in AddIdentity();
            // IdentityRole - is also a builtin class provided by ASP.NET Core Identity and contains Role information.
            // Since we are created the custom IdentityUser class we need specoify wheneever we are register IdentityService #1.
            services.AddIdentity<ApplicationUser, IdentityRole>(options=>

                //  Here we need to add specific that while genereta emailconfirmation token use the custom tokenprovider
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfimation"
            )
            .AddEntityFrameworkStores<AppDbContext>()
            // This is to add a default token provider that generates taoken for eamil confirmation ,2 factor authentication etc
            .AddDefaultTokenProviders()
            // Here we are adding the CustomTokenProvider in order specify Lifespan specific to EmailConfirmation Token
            .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfimation");

            // IdentityOptions are basically used to configure PasswordOptions,Useroptions,SignInOptions,LockoutOptions,TokenOptions,StoreOptions,ClaimsIdentityOptions
            // By default our app allow to create a simple password let add Passwordoptions so that it accepts what we specififed.
            // To indicate rules use PasswordOptions ,this options can be also passed as parameter to the AddIdentity() also above.
            services.Configure<IdentityOptions>((options) =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;

                // Here we need to make user to be confirmed his provide email is belong to him before he signin to our app,if email is not confirmed we need to block signin
                // To restrict the user to Confirm email set RequireConfirmEmail property to true in the SignOptions
                options.SignIn.RequireConfirmedEmail = true;
            });

            // Claims Based Authorization means using claims to make some access control decesion.
            // Step 1 . Create Policy from by using one or more claims and register those policy as Authorization options // Step 2 . Use That policy on controller or action
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role", "true"));
                // options.AddPolicy("EditRolePolicy", policy => policy
                //                                     .RequireRole("Admin")
                //                                     .RequireClaim("Edit Role", "true")
                //                                     .RequireRole("Super Admin") but if we use this we get
                //  );
                
                // using func creating custom authorization policy
                 options.AddPolicy("Edit RolePolicy",policy=> policy.RequireAssertion(context=>AuthorizeAccess(context)));

                // We have to add Custom AuthorizationRequirement to our policy ,with the custom AuthorizationHandler
                options.AddPolicy("EditRolePolicy",policy=>policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));


                options.AddPolicy("CreateRolePolicy", policy => policy.RequireClaim("Create Role", "true"));

                // Here if we want any remaning handlers would not be called if the any one is returing explict faliure
                // options.InvokeHandlersAfterFailure = false;
             });
            // inorder to delete role logged in user must be having claim specified by policy


            // If we need to change the default AccessDenied route /Account/AccessDenied path to Some custom Route like Administartion/AccessDenied path
            services.ConfigureApplicationCookie(options =>
            {
                //  options.LoginPath = new PathString("/Account/Login");
                // you can configur denied route path however you want
                 options.AccessDeniedPath= new PathString("/Administration/AccessDenied");
            });


            // we are registeriog custom AuthorizationHandler so that we use with custom AuthorizationRequirement
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            // this is second handler registered with ManageAdminRolesAndClaimsRequirement,so we have register with IOC
            services.AddSingleton<IAuthorizationHandler,SuperAdminHandler>();


            // Here we are Configuring the Googel as External identity provider for our application
            services.AddAuthentication().AddGoogle(options=>{
                // client id and secret we get it after registeration with Google external identity provider and settiong oAuth consent screen,oAuth client credentials
                options.ClientId = "226970152231-49a2gdupc5u188fif0qbr4u2oo5mq14e.apps.googleusercontent.com";
                options.ClientSecret = "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";
            })
            .AddFacebook(options=>{
                options.AppId = "820021739102576";
                options.AppSecret = "dfcf1084985deedea8a2671b304c38a3";
            });
 
            // If we need configure Tokens ex: PasswordresetTokens/EmailConfirmationsTokens lifespan instaed of default(1day) lifespan some hours,
            // DataProtectorTokenProvider class of framework is responsibly for generate (using generate() method) and Validate (using Validate() method) Tokens,this methods are get called by GenerateUserToken(user,tokenprovider,tokenpuprose) which is internally get called by UserManager.GeneratePasswordResetTokenAsync()
            // If we set lifespan using the below it will set it for all kind Tokens this custom lifespan
            services.Configure<DataProtectionTokenProviderOptions>(o=>o.TokenLifespan = TimeSpan.FromHours(1));

            // once we registered the custom email confirmation token use that to specify the lifespan for Emailconfirmation token
            // Changes token lifespan of just the Email Confirmation Token type
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o=>o.TokenLifespan = TimeSpan.FromDays(3));
            
            // Here we are registering the DataProtectionPurpose string with DI so that we can use those purpose strings in CreateProtector() of IDataProtectionProvider class,get an IDataProtector
            services.AddSingleton<DataProtectionPurposeStrings>();

            // Here we are configure an Account Lockout options so that an account will be locked after this specific attempts
            services.Configure<LockoutOptions>(options=>{
               //here account need locked after 5 failed logon attemps
                options.MaxFailedAccessAttempts = 5;
                // account need to lock for 15mins
                options.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            });
        }

        //HTTP request pipeline of app using middleware.
        [Obsolete]
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                DeveloperExceptionPageOptions options = new DeveloperExceptionPageOptions();
                options.SourceCodeLineCount = 2;

                // This is the Diagnostic middleware,that captures Exceptions and generates HTML error response.
                app.UseDeveloperExceptionPage(options);
            }
            else
            {
                // Here catch and to replace default error that with 500statuscode is returned if exception is caught in other than Development environment.
                app.UseExceptionHandler("/Error");
            }
            //   This is used to handle the unhandled 404 error and return custom error pages instead of default
            //  The placeholder {0}, in "/Error/{0}" will automatically receive the http status code.
            app.UseStatusCodePagesWithReExecute("/Error/{0}");



            // This is our custom middleware.
            // app.UseMyMiddleware();

            // This is the serve static files
            app.UseStaticFiles();


            // We want to be able to authenticate users before the request reaches the MVC middleware. So it's important we add authentication middleware before the MVC middleware in the request processing pipeline.
            app.UseAuthentication();
            app.UseAuthorization();


            // This middleware required for MVC
            app.UseMvcWithDefaultRoute();
            // app.UseMvc((routesbuilder)=>{
            //     routesbuilder.MapRoute("default","{controller=Home}/{action=Index}/{id?}");
            // });







































            // app.Use(
            //     async (context,next) =>
            //     {
            //         await context.Response.WriteAsync("This request is handled by 1st Middeleware.");
            //         await next();
            //     }
            // );



            // app.Run(
            //     async (context) =>
            //     {
            //         // await context.Response.WriteAsync("\nThis request is handled by 2st Middeleware.");
            //         throw new Exception("Some Error Occured...");
            //     }
            // );
        }

        // customizie policy using func
        private bool AuthorizeAccess(AuthorizationHandlerContext context)
        {
            return context.User.IsInRole("Admin") && context.User.HasClaim("Edit Role","true")
                    ||
                   context.User.IsInRole("Super Admin");
                 
        }

    }
}