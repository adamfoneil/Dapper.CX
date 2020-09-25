$(document).ready(function () {
    $(".code-sample").each(async function (index) {
        var url = $(this).data("url");
        const response = await fetch(url);
        const content = await response.text();        
        $(this).text(content);                
    });    
});