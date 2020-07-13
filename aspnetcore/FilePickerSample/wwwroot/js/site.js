(function() {
    console.log("Javascript loaded.");
})();

function openFilePicker() {
    var height = document.getElementById('input_height').value;
    var width = document.getElementById('input_width').value;

    var config = {
        apiKey: 'fake-api-key',
        onFileSelected: function (fileRoute) {
            console.log('File selected: ' + fileRoute);
        },
    };

    if (height) {
        config.height = height + 'px';
    }

    if (width) {
        config.width = width + 'px';
    }

    window.initializePowerDmsFilePicker(config);
}
