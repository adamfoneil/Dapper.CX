$(document).ready(function () {
    let blocks = document.querySelectorAll(".code-sample");
    blocks.forEach(async function (ele) {
        let url = $(ele).data("url");
        const response = await fetch(url);
        const content = await response.text();
        ele.innerText = content;
        hljs.highlightBlock(ele);

        let importElement = $(ele).data("import-element");
        if (importElement != null) {
            let importContent = document.getElementById(importElement);
            let target = document.getElementById(importElement + "-target");
            target.appendChild(importContent);
            document.removeChild(importContent);
        }
    });    
});