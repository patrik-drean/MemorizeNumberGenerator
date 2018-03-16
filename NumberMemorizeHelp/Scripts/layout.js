$(document).ready(function () {
    var url = window.location;
    $('ul.nav a[href="' + url + '"]').addClass('active2');
    $('ul.nav a').filter(function () {
        return this.href == url;
    }).addClass('active2');
});

