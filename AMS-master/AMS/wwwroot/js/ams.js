setTimeout(function () {

    // Closing the alert
    $('.alert').alert('close');
}, 5000);

function aTagClick(sel) {
    console.log('clicked')
    var id = sel.id;
    console.log(id);
    $('#AtagDiv a').removeClass('active');
    $('#' + id).addClass('active');
}

