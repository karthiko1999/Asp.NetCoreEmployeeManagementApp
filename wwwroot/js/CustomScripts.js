// Here this is script to hide and show a span delete
function confirmDelete(userId,isDeleteClicked)
{
    var confirmDeleteSpan = 'confirmDeleteSpan_'+userId;
    var deleteSpan = 'deleteSpan_'+userId;

    if(isDeleteClicked){
        // if the delete is clicked

        // we need use span d selector and show or hide 
        $('#'+confirmDeleteSpan).show();
        $('#'+deleteSpan).hide();   
    }
    else
    {
        $('#'+confirmDeleteSpan).hide();
        $('#'+deleteSpan).show(); 
    }
}
