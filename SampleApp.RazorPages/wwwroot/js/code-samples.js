$(document).ready(function () {
    let blocks = document.querySelectorAll(".code-sample");
    blocks.forEach(async function (ele) {

        let jsonEncoded = $(ele).data("source-request");
        let request = decodeJson(jsonEncoded);        
       
        const response = await fetch("/GitHub/Source", {
            method: "post",
            body: JSON.stringify(request)
        });

        const content = await response.text();
        ele.innerHTML = content;        
        hljs.highlightBlock(ele);
        $(ele).tooltip({
            items: "span.sample-tooltip",
            content: function () {
                let contentId = $(this).data("tooltip");
                let node = document.getElementById(contentId).cloneNode(true);
                node.style.display = "block";
                return node;
            }
        });

        let importElement = $(ele).data("import-element");
        if (importElement != "") {
            let importContent = document.getElementById(importElement);
            let target = document.getElementById(importElement + "-target");
            target.appendChild(importContent);            
        }
    });  

    blocks = document.querySelectorAll(".code-block");
    blocks.forEach(function (ele) {
        hljs.highlightBlock(ele);
    });

    
    
});

function decodeJson(input) {
    let json = replaceAll(input, "&quot;", "\"");
    json = replaceAll(json, "&amp;", "&");
    return JSON.parse(json);
}

function replaceAll(input, lookFor, replaceWith) {
    let tokens = input.split(lookFor);
    return tokens.join(replaceWith);
}
