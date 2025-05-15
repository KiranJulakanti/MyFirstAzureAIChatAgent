"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

var input = document.getElementById("userInput");

// Execute a function when the user presses a key on the keyboard
input.addEventListener("keypress", function (event) {
    // If the user presses the "Enter" key on the keyboard, cancel default action
    if (event.key === "Enter") {
        event.preventDefault();
        document.getElementById("sendButton").click();
    }
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = "test";

    var container = document.getElementById("containerDiv");
    var newDiv = document.createElement("div");
    newDiv.style.backgroundColor = "#f0f0f0";
    newDiv.style.height = "auto";
    newDiv.innerHTML = "User: " + user
    container.appendChild(newDiv);

    document.getElementById("userInput").value = '';

    // Call the SendMessage method on the hub.
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

connection.on("ReceiveMessage", function (user, agent) {

    var container = document.getElementById("containerDiv");
    var newDiv = document.createElement("div");
    newDiv.style.textAlign = "right";
    newDiv.style.height = "auto";
    newDiv.innerHTML = "Agent: " + agent

    container.appendChild(newDiv);
});

function ProvideDetails(){
    const tableHtml = `
        <table border="1">
          <tr>
            <td><label for="customerName">Customer Name</label></td>
            <td><input type="text" id="customerName" /></td>
          </tr>
          <tr>
            <td><label for="customerTaxId">Customer Tax Id</label></td>
            <td><input type="text" id="customerTaxId" /></td>
          </tr>
          <tr>
            <td colspan="2" style="text-align: center;">
              <input type="button" id="submitButton" value="Submit" onclick=""/>
            </td>
          </tr>
        </table>`;

    var container = document.getElementById("containerDiv");
    var newDiv = document.createElement("div");
    newDiv.style.backgroundColor = "#f0f0f0";
    newDiv.style.height = "auto";
    newDiv.innerHTML = tableHtml
    container.appendChild(newDiv);


    //var user = document.getElementById("userInput");
    //user.value = "CustomerName:____, CustomerTaxId:_____";
}

// Button click handler
document.addEventListener("click", function (event) {
    if (event.target && event.target.id === "submitButton") {
        const name = document.getElementById("customerName").value;
        const taxId = document.getElementById("customerTaxId").value;

        var customerDetails = "{'CustomerName':'" + name + "', 'CustomerTaxId':'" + taxId + "'}";

        // Call the SendMessage method on the hub.
        connection.invoke("SendMessage", customerDetails, "test").catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();

        // Call server-side method via SignalR
        //connection.invoke("SubmitCustomerData", name, taxId)
        //    .then(() => console.log("Data sent to server"))
        //    .catch(err => console.error(err.toString()));
    }
});
