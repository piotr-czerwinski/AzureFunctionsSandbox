using System;
using System.Collections.Generic;
using System.Text;

namespace TickerInformator
{
    public class StaticStringHTMLComposer : IHTMLComposer
    {
        public string BuildSubscribeFormHTML()
        {
            string hostUrl = Environment.GetEnvironmentVariable("HostUrl");
            return
@"
<!doctype html>
<html lang=""pl"">
<head>
    <meta charset=""utf-8"">
    <title>Subscribe</title>
    <script src=""https://code.jquery.com/jquery-1.10.2.js""></script>
</head>
<body>
<h1>Ticker info</h1>
<p>Enter e-mail and alert threshold for BTC/USD change (checked hourly 8-20). Set threshold to '0' to unsubscribe!</p>
<form action=""" + hostUrl + @"/SubmitSubscription"" id=""searchForm"">
<table>
    <tr>    
        <td>
            E-mail:
        </td>
        <td>
            <input type=""email"" name=""email"" />
        </td>
    </tr>
    <tr> 
        <td>
            Alert treshold:
        </td>
        <td>
            <input type=""number"" name=""alertTreshold"" min=""0"" max=""99"" value =""5"">% (+-)
        </td>
    </tr>
</table>
<input type=""submit"" value=""Send!"" />
</form>
<br />
<div style=""padding-top: 10px;"" id=""result""></div>
<script>
$( ""#searchForm"" ).submit(function( event ) {
    event.preventDefault();
 
    var $form = $( this ),
    email = $form.find( ""input[name='email']"" ).val(),
    alertTreshold = $form.find( ""input[name='alertTreshold']"" ).val(),
    url = $form.attr( ""action"" );
 
    // Send the data using post
    var posting = $.post( url, JSON.stringify({ ""email"": email, ""alertTreshold"" : alertTreshold}) );
 
    // Put the results in a div
    posting.done(function( data ) {
    $( ""#result"" ).empty().append( data );
    });
});
</script>
</body>";
        }
    }
}
