
function convertImages(sourceDir, destDir) {
    var samplesFolder = Folder(sourceDir);
    var fileList = samplesFolder.getFiles("*.psd");

    // open each file
    for (var i = 0; i < fileList.length; i++) {
        // save the file as a photoshop file
        open(fileList[i]);

        // export the file as a png file
	var fname = fileList[i].name;
	var idx = fname.indexOf(".psd");
	if (idx != -1) {
	    fname = fname.substring(0, idx);
        }
        var targetPngFile = new File(destDir + fname + ".tga");
        var saveOptions = new TargaSaveOptions();
        saveOptions.alphaChannels = true;
        saveOptions.resolution = TargaBitsPerPixels.THIRTYTWO;
        app.activeDocument.saveAs(targetPngFile, saveOptions);

        // don't save anything we did
        app.activeDocument.close(SaveOptions.DONOTSAVECHANGES);

        // close the file
        fileList[i].close();
    }
}

function convertImageSet(sourceDir, destDir) {
    var sourceFolder = new Folder(sourceDir);
    var dirList = sourceFolder.getFiles();
    for (var i = 0; i < dirList.length; i++) {
        convertImages(dirList[i].path + "/" + dirList[i].name, targetDir);
    }
}

// Save the current preferences
var startRulerUnits = app.preferences.rulerUnits;
var startTypeUnits = app.preferences.typeUnits;
var startDisplayDialogs = app.displayDialogs;

// Set Adobe Photoshop CS2 to use pixels and display no dialogs
app.preferences.rulerUnits = Units.PIXELS;
app.preferences.typeUnits = TypeUnits.PIXELS;
app.displayDialogs = DialogModes.NO;

// Close all the open documents
while (app.documents.length) {
    app.activeDocument.close();
}

// Set up the source and target directories
var targetDir = "C:/rocketbox_textures/";
convertImageSet("C:/rocketbox/male_characters_1/", targetDir)
convertImageSet("C:/rocketbox/male_characters_2/", targetDir)
convertImageSet("C:/rocketbox/female_characters/", targetDir)

// Reset the application preferences
app.preferences.rulerUnits = startRulerUnits;
app.preferences.typeUnits = startTypeUnits;
app.displayDialogs = startDisplayDialogs;
