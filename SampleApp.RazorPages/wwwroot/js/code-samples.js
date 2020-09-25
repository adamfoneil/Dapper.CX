$(document).ready(function () {
    let blocks = document.querySelectorAll(".code-sample");
    blocks.forEach(async function (ele) {
        var url = $(ele).data("url");
        const response = await fetch(url);
        const content = await response.text();
        ele.innerText = content;
        hljs.highlightBlock(ele);
    });    
});