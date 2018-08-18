
var loadGXCalc = function () {  
    gxCalcInit();
   
}

var loadGXCalcInit = function () {
    $.ajax({
        type: 'GET',
        url: '@Url.Action("GXCalcInit", "Home")',
        dataType: 'html',
        beforeSend: function () {
            alert("start init()");
            var gxcalcNode = document.getElementById("gxcalcmain");
            var gxcalcLoading = document.createTextNode("Loading widget....");
            gxcalcNode.appendChild(gxcalcLoading);
        },
        success: function (data) {
            $('#gxcalcmain').html(result);
        },
        complete: function () {
            //alert("stop");
            //$(id).find('i').removeClass('fa-spin');
            $body.removeClass("loading");
        }
    });
}

var gxCalcInit = function () {
    var xhr = new XMLHttpRequest();
    xhr.open('GET', 'http://localhost:58961/Home/GXCalcInit');
    xhr.onload = function () {
        if (xhr.status === 200) {            
            $('#gxCalcMain').html(xhr.responseText);
        }
        else {
            $('#gxCalcMain').html("oops ! GX Calculator Error failed to load");
        }
    };
    xhr.send();
}
