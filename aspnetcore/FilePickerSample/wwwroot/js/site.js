(function() {
    console.log("Javascript loaded.");
})();

function openFilePicker() {
    var height = document.getElementById('input_height').value;
    var width = document.getElementById('input_width').value;

    var config = {
        apiKey: 'fake-api-key',
        onSelection: displayResponse,
    };

    if (height) {
        config.height = Number(height);
    }

    if (width) {
        config.width = Number(width);
    }

    window.PowerDms.initializePowerDmsFilePicker(config);
}

function displayResponse(response) {
    var json = JSON.stringify(response, null, 2);
    var message = 'FilePicker says: \n' + json;
    alert(message);
}
