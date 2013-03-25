function ImageInfo(file) {
  this.file = file;
  this.width = 0;
  this.height = 0;
  this.xOffset = 0;
  this.yOffset = 0;
}

function Size() {
  this.width = 0;
  this.height = 0;
}

function printChannels() {
  var channelInfo;
  var channels;
  
  channelInfo = "All Channels";
  channels = app.activeDocument.channels;
  for (var i = 0; i < channels.length; ++i) {
    var channel = channels[i];
    channelInfo = channelInfo + "\n" + channel.name + ": " + channel.typename + ", " + channel.kind;
  }
  alert(channelInfo);
  
  channelInfo = "Active Channels";
  channels = app.activeDocument.activeChannels;
  for (var i = 0; i < channels.length; ++i) {
    var channel = channels[i];
    channelInfo = channelInfo + "\n" + channel.name + ": " + channel.typename + ", " + channel.kind;
  }
  alert(channelInfo);
}

function activateChannels(doc) {
  // activate all channels
  for (var i = 0; i < doc.channels.length; ++i) {
    doc.activeChannels[i] = doc.channels[i];
  }
  alert("channels activated");
}

function isValidSourceFile(srcFile) {
  if (srcFile instanceof File) {
    if (srcFile.name.match(".(tga|png)$")) {
      return true;
    }
  }
  return false;
}

function getImageSizes(sourceFolder, imageset) {
  var imageInfoList = [];
  // Use the path to the source folder and append the imageset folder
  var matchingFiles = sourceFolder.getFiles(imageset);
  var samplesFolder;
  if (matchingFiles.length == 1) {
    if (matchingFiles[0] instanceof Folder) {
      samplesFolder = matchingFiles[0];
    }
  }
  if (!samplesFolder) {
    alert('Unable to access imageset: ' + imageset);
    return;
  }
  sourceFolder.getFiles(imageset);
  //Get all the files in the folder
  var fileList = samplesFolder.getFiles(isValidSourceFile);
  
  // open each file
  for (var i = 0; i < fileList.length; i++) {
    // The fileList is folders and files so open only files
    if (!(fileList[i] instanceof File)) {
      continue;
    }
    open(fileList[i]);
    
    var imageInfo = new ImageInfo(fileList[i]);
    // get the dimensions of this image
    imageInfo.width = app.activeDocument.width.value;
    imageInfo.height = app.activeDocument.height.value;
    // don't save anything we did
    app.activeDocument.close(SaveOptions.DONOTSAVECHANGES);
    
    imageInfoList[imageInfoList.length] = imageInfo;
    // close the file
    fileList[i].close();
  }
  
  return imageInfoList;
}

function computeImagesetSize(imageInfoList, size) {
  var docWidth = size;
  var docHeight = size;
  var padSize = 1;
  var xOffset = 0;
  var yOffset = 0;
  var maxLayerHeight = 0;
  var dims = new Size();
  dims.width = size;
  dims.height = size;
  // open each file
  for (var i = 0; i < imageInfoList.length; i++) {
    // get the dimensions of this new layer
    var layerWidth = imageInfoList[i].width;
    var layerHeight = imageInfoList[i].height;
    
    // see if the layer will fit on this line
    if (xOffset + layerWidth > docWidth) {
      if (layerWidth > docWidth) {
        return false;
      }
      xOffset = 0;
      yOffset = yOffset + maxLayerHeight + 2 * padSize;
      maxLayerHeight = 0;
    }
    if (layerHeight > maxLayerHeight) {
      maxLayerHeight = layerHeight;
    }
    
    // Store the computed x and y offsets
    imageInfoList[i].xOffset = xOffset;
    imageInfoList[i].yOffset = yOffset;
    
    xOffset = xOffset + layerWidth + 2 * padSize;
    
    if (yOffset + layerHeight > size) {
      return false;
    }
  }
  
  var tmp = dims.width;
  while (yOffset + maxLayerHeight <= tmp && tmp > 0) {
    dims.height = tmp;
    tmp = tmp / 2;
  }
  if (dims.height == 0) {
    alert(imageInfoList[0].file.name);
    return false;
  }
  
  return dims;
}

function getImageInfo(imageInfoList, file) {
  for (var i = 0; i < imageInfoList.length; ++i) {
    if (imageInfoList[i].file.name == file.name) {
      return imageInfoList[i];
    }
  }
}

function makeRegion(x, y, w, h) {
  return new Array(Array(x, y),
		   Array(x + w, y),
		   Array(x + w, y + h),
		   Array(x, y + h),
		   Array(x, y));
}

function copyRegion(dstDoc, dstChannels, dstRegion, srcDoc, srcChannels, srcRegion) {
  app.activeDocument = srcDoc;
  srcDoc.activeChannels = srcChannels;
  srcDoc.selection.select(srcRegion);
  srcDoc.selection.copy();

  app.activeDocument = dstDoc;
  dstDoc.activeChannels = dstChannels;
  dstDoc.selection.select(dstRegion);
  dstDoc.paste();
  dstDoc.activeLayer.opacity = 100;
  return dstDoc.activeLayer;
}

function buildImageset(imageInfoList, sourceFolder, targetFolder, imageset, dims) {
  // Create a new document to merge all the samples into
  var mergedDoc = app.documents.add(dims.width, dims.height, 72, imageset,
				    NewDocumentMode.RGB, DocumentFill.TRANSPARENT, 1);
  var alphaChannel = mergedDoc.channels.add();
  alphaChannel.name = "Alpha 1";
  alphaChannel.kind = ChannelType.MASKEDAREA;
  //Get all the files in the folder
  var docWidth = app.activeDocument.width.value;
  var docHeight = app.activeDocument.height.value;
  var imagesetXml = "";
  imagesetXml = imagesetXml + "<?xml version=\"1.0\" ?>\n";
  imagesetXml = imagesetXml + "<Imageset Name=\"" + imageset + "\" Imagefile=\"" + imageset + ".tga\" NativeHorzRes=\"" + docWidth + "\" NativeVertRes=\"" + docHeight + "\" AutoScaled=\"false\">\n";
  var channelSets = new Array(2);
  channelSets[0] = new Array("Red", "Green", "Blue");
  channelSets[1] = new Array("Alpha 1");
  // open each file
  var foundFile = false;
  for (var i = 0; i < imageInfoList.length; i++) {
    foundFile = true;
    open(imageInfoList[i].file);
    var imageInfo = imageInfoList[i];
    // use the document name for the layer name in the merged document
    var singleDoc = app.activeDocument;
    var docName = singleDoc.name;
    docName = docName.substring(0, docName.length - 4);
    // flatten the document so we get all the layers, and then copy
    singleDoc.flatten();

    // get the dimensions of this new layer
    for (var j = 0; j < channelSets.length; ++j) {
      var srcRegion, dstRegion;
      
      srcChannels = new Array(channelSets[j].length);
      for (var k = 0; k < channelSets[j].length; ++k) {
	var has_channel = false;
	for (var l = 0; l < singleDoc.channels.length; ++l) {
	  if (singleDoc.channels[l].name == channelSets[j][k]) {
	    has_channel = true;
	    break;
	  }
	}
	if (!has_channel) {
	  app.activeDocument = singleDoc;
	  var newChannel = singleDoc.channels.add(channelSets[j][k]);
	  if (channelSets[j][k] == "Alpha 1") {
	    newChannel.kind = ChannelType.MASKEDAREA;
	  }
	  app.activeDocument = mergedDoc;
	}
	srcChannels[k] = singleDoc.channels.getByName(channelSets[j][k]);
      }
      
      dstChannels = new Array(channelSets[j].length);
      for (var k = 0; k < channelSets[j].length; ++k) {
	dstChannels[k] = mergedDoc.channels.getByName(channelSets[j][k]);
      }

      // First copy the main portion of the image
      if (imageInfo.width != 0 || imageInfo.height != 0) {
	srcRegion = makeRegion(0, 0, imageInfo.width, imageInfo.height);
	dstRegion = makeRegion(imageInfo.xOffset, imageInfo.yOffset, imageInfo.width, imageInfo.height);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	// set the layer name
	mergedDoc.activeLayer.name = docName;
      }
      // copy the left edge
      if (imageInfo.xOffset != 0) {
	srcRegion = makeRegion(0, 0, 1, imageInfo.height);
	dstRegion = makeRegion(imageInfo.xOffset - 1, imageInfo.yOffset, 1, imageInfo.height);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-left";
      }
      // copy the right edge
      if (imageInfo.xOffset + imageInfo.width < docWidth) {
	srcRegion = makeRegion(imageInfo.width - 1, 0, 1, imageInfo.height);
	dstRegion = makeRegion(imageInfo.xOffset + imageInfo.width, imageInfo.yOffset, 1, imageInfo.height);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-right";
      }
      // copy the top edge
      if (imageInfo.yOffset > 0) {
	srcRegion = makeRegion(0, 0, imageInfo.width, 1);
	dstRegion = makeRegion(imageInfo.xOffset, imageInfo.yOffset - 1, imageInfo.width, 1);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-top";
      }
      // copy the bottom edge
      if (imageInfo.yOffset + imageInfo.height < docHeight) {
	srcRegion = makeRegion(0, imageInfo.height - 1, imageInfo.width, 1);
	dstRegion = makeRegion(imageInfo.xOffset, imageInfo.yOffset + imageInfo.height, imageInfo.width, 1);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-bottom";
      }
      // copy the top left corner
      if (imageInfo.xOffset > 0 && imageInfo.yOffset > 0) {
	srcRegion = makeRegion(0, 0, 1, 1);
	dstRegion = makeRegion(imageInfo.xOffset - 1, imageInfo.yOffset - 1, 1, 1);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-topleft";
      }
      // copy the top right corner
      if (imageInfo.xOffset + imageInfo.width < docWidth && imageInfo.yOffset > 0) {
	srcRegion = makeRegion(imageInfo.width - 1, 0, 1, 1);
	dstRegion = makeRegion(imageInfo.xOffset + imageInfo.width, imageInfo.yOffset - 1, 1, 1);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-topright";
      }
      // copy the bottom left corner
      if (imageInfo.xOffset > 0 && imageInfo.yOffset + imageInfo.height < docHeight) {
	srcRegion = makeRegion(0, imageInfo.height - 1, 1, 1);
	dstRegion = makeRegion(imageInfo.xOffset - 1, imageInfo.yOffset + imageInfo.height, 1, 1);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-bottomleft";
      }
      // copy the bottom right corner
      if (imageInfo.xOffset + imageInfo.width < docWidth && imageInfo.yOffset + imageInfo.height < docHeight) {
	srcRegion = makeRegion(imageInfo.width - 1, imageInfo.height - 1, 1, 1);
	dstRegion = makeRegion(imageInfo.xOffset + imageInfo.width, imageInfo.yOffset + imageInfo.height, 1, 1);
	copyRegion(mergedDoc, dstChannels, dstRegion, singleDoc, srcChannels, srcRegion);
	mergedDoc.activeLayer.name = docName + "-bottomright";
      }
      
      // mergedDoc.activeLayer = newLayer;
    }
    // don't save anything we did
    singleDoc.close(SaveOptions.DONOTSAVECHANGES);
    
    // add an entry to the xml string
    imagesetXml = imagesetXml + "    <Image Name=\"" + docName + "\" XPos=\"" + imageInfo.xOffset + "\" YPos=\"" + imageInfo.yOffset + "\" Width=\"" + imageInfo.width + "\" Height=\"" + imageInfo.height + "\" />\n"; 
    // close the file
    imageInfoList[i].file.close();
  }
  if (!foundFile) {
    // close the active document and the file
    mergedDoc.close(SaveOptions.DONOTSAVECHANGES);
    return;
  }
  // save the file as a photoshop file
  var targetPsdFile = new File(targetFolder.fsName + "/" + imageset + ".psd");
  mergedDoc.saveAs(targetPsdFile);
  // export the file as a png file
  var targetPngFile = new File(targetFolder.fsName + "/" + imageset + ".tga");
  var saveOptions = new TargaSaveOptions();
  saveOptions.alphaChannels = true;
  saveOptions.resolution = TargaBitsPerPixels.THIRTYTWO;
  mergedDoc.saveAs(targetPngFile, saveOptions);
  // close the active document and the file
  mergedDoc.close(SaveOptions.DONOTSAVECHANGES);
  // close out the xml tag
  imagesetXml = imagesetXml + "</Imageset>\n";
  // write the xml to the file
  var targetXmlFile = new File(targetFolder.fsName + "/" + imageset + ".xml");
  targetXmlFile.open("w");
  targetXmlFile.write(imagesetXml);
  targetXmlFile.close();
}

function processFolder(sourceFolder, targetFolder, folder) {
  // The fileList is folders and files so open only files
  if (!(folder instanceof Folder)) {
    return;
  }
  var imageInfoList = getImageSizes(sourceFolder, folder.name);
  if (imageInfoList.length == 0) {
    return;
  }
  var tmp;
  tmp = computeImagesetSize(imageInfoList, 512);
  if (!tmp) {
    tmp = computeImagesetSize(imageInfoList, 1024);
    if (!tmp) {
      tmp = computeImagesetSize(imageInfoList, 2048);
    }
  }
  if (tmp) {
    buildImageset(imageInfoList, sourceFolder, targetFolder, folder.name, tmp);
  }
}

function buildImagesets(defaultSourceDir, defaultTargetDir) {
  // Set up the source and target directories
  var sourceFolder = Folder.selectDialog("Choose the folder with source files", defaultSourceDir);
  if (sourceFolder == null) {
    return 'Cancelling conversion';
  }
  var targetFolder = Folder.selectDialog("Choose the folder to save the imageset files", defaultTargetDir);
  if (targetFolder == null) {
    return 'Cancelling conversion';
  }
  
  var samplesFolder = sourceFolder;
  // Get all the files in the folder
  var fileList = samplesFolder.getFiles();
  // open each file
  for (var i = 0; i < fileList.length; i++) {
    // if (fileList[i].name == "Durability") {
    processFolder(sourceFolder, targetFolder, fileList[i]);
    // }
  }
  return;
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

var sourceDir = "C:/Interface/";
var targetDir = "C:/NewInterface/";
var err_msg = buildImagesets(sourceDir, targetDir);
if (err_msg) {
  alert(err_msg);
}

// Reset the application preferences
app.preferences.rulerUnits = startRulerUnits;
app.preferences.typeUnits = startTypeUnits;
app.displayDialogs = startDisplayDialogs;
