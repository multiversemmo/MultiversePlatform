using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
//using Axiom.Core;
//using Axiom.Serialization;
using System.Diagnostics;
using Microsoft.Win32;

namespace Multiverse.AssetRepository
{

    public enum AssetTypeEnum {
		None = 0,
		Mesh,
		Material,
		SpeedTree,
		SpeedWind,
		PlantType,
		ParticleScript,
		Sound,
		Font,
		Icon,
		FrameXML,
		ImageSet,
        PythonScript,
		Mosaic,
        Movie,
        Texture,
		Other
	};

	public enum AssetStatusEnum {
		None = 0,
		Complete,
		Incomplete,
		Broken
	};

	public enum AssetFileEnum {
		None = 0,
		Mesh,
		Material,
		Skeleton,
		Texture,
		Physics,
		Shader,
		ParticleScript,
		XML,
		PythonScript,
		ImageSet,
		Font,
		Icon,
		SpeedTree,
		SpeedWind,
		Sound,
		AssetDef,
		Mosaic,
        Movie,
		Other
	};

	public enum AssetFileStatus {
		None = 0,
		InRepository,
		AddedToRepository
	};
	
	public enum ExtensionPrimaryEnum {
		DontCare = 0,
		MatchPrimary,
		DontMatchPrimary
	}
	
	// An Xml writer that lets me control indentation, since the
	// XmlSettings.indent stuff doesn't seem to work.
    //public class MyXmlWriter
    //{
    //    public MyXmlWriter(string path)
    //    {
    //        indent = "";
    //        FileStream f = new FileStream(path, FileMode.Create, FileAccess.Write);
    //        writer = new StreamWriter(f);
    //        writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    //    }
		
    //    public void MaybeOutAttribute(string attributeName, string value)
    //    {
    //        if (value.Length > 0)
    //            writer.WriteLine(string.Format("{0}<{1}>{2}</{1}>", indent, attributeName, value));
    //    }
		
    //    public void WriteStartElement(string elementName)
    //    {
    //        writer.WriteLine(string.Format("{0}<{1}>", indent, elementName));
    //        indent += indentIncrement;
    //    }
		
    //    public void WriteEndElement(string elementName)
    //    {
    //        indent = indent.Substring(indentIncrement.Length);
    //        writer.WriteLine(string.Format("{0}</{1}>", indent, elementName));
    //    }
		
    //    public void WriteElementWithAttributes(string elementName, string attributes)
    //    {
    //        writer.WriteLine(string.Format("{0}<{1} {2} />", indent, elementName, attributes));
    //    }
		
    //    public void Close()
    //    {
    //        //Assert.AreEqual(indent.Length, 0);
    //        writer.Close();
    //    }
		
    //    protected StreamWriter writer;
    //    protected string indent;
    //    protected const string indentIncrement = "  ";
    //}
	
	// A mapping between file extensions and the file type enums they
	// represent.  Also supplies a mapping from the triple 
	// (asset type, primary file, extension/enum) to/from the 
	// directory in which such a file should be stored.
	public class ExtensionMapping
	{
		public ExtensionMapping(AssetTypeEnum assetType, string fileExtension,
								string assetDirectory, AssetFileEnum fileTypeEnum)
		{
			this.primaryMatchEnum = ExtensionPrimaryEnum.DontCare;
			this.typeEnum = assetType;
			this.fileExtension = fileExtension;
			this.assetDirectory = assetDirectory;
			this.fileTypeEnum = fileTypeEnum;
		}

		public ExtensionMapping(AssetTypeEnum assetType, bool primaryMatch, string fileExtension,
								string assetDirectory, AssetFileEnum fileTypeEnum)
		{
			this.primaryMatchEnum = (primaryMatch ? ExtensionPrimaryEnum.MatchPrimary :
									 ExtensionPrimaryEnum.DontMatchPrimary);
			this.typeEnum = assetType;
			this.fileExtension = fileExtension;
			this.assetDirectory = assetDirectory;
			this.fileTypeEnum = fileTypeEnum;
		}

		public AssetTypeEnum AssetType
		{
			get { return typeEnum; }
		}
		
		public string FileExtension
		{
			get { return fileExtension; }
		}
				
		public string AssetDirectory
		{
			get { return assetDirectory; }
		}
				
		public AssetFileEnum FileTypeEnum
		{
			get { return fileTypeEnum; }
		}
		
		public ExtensionPrimaryEnum PrimaryMatch
		{
			get { return primaryMatchEnum; }
		}
		
		protected AssetTypeEnum typeEnum;
		protected string fileExtension;
		protected string assetDirectory;
		protected AssetFileEnum fileTypeEnum;
		protected ExtensionPrimaryEnum primaryMatchEnum;
	}
	
	// This class describes one (collection of) files in an
	// AssetTypeDesc.  That collection has a file type enum, and a min
	// and max count.  AssetTypeFileDesc/AssetTypeDesc are asset type
	// definitions; asset definition instances are represented in
	// AssetFile and AssetDefinition.
	public class AssetTypeFileDesc 
	{
		public AssetTypeFileDesc(AssetFileEnum fileTypeEnum, int minCount, int maxCount, 
								 string additionalText)
		{
			this.additionalText = additionalText;
			this.fileTypeEnum = fileTypeEnum;
			this.minCount = minCount;
			this.maxCount = maxCount;
		}

		public AssetTypeFileDesc(AssetFileEnum fileTypeEnum, int minCount, int maxCount)
		{
			this.additionalText = "";
			this.fileTypeEnum = fileTypeEnum;
			this.minCount = minCount;
			this.maxCount = maxCount;
		}

		public AssetFileEnum FileTypeEnum
		{
			get { return fileTypeEnum; }
			set { fileTypeEnum = value; }
		}
		
		public int MinCount
		{
			get { return minCount; }
			set { minCount = value; }
		}
		
		public int MaxCount
		{
			get { return maxCount; }
			set { maxCount = value; }
		}
		
		public string AdditionalText
		{
			get { return additionalText; }
			set { additionalText = value; }
		}

		public string FileExtension
		{
			get { return AssetFile.ExtensionForEnum(fileTypeEnum); }
		}
			
		protected AssetFileEnum fileTypeEnum;
		protected int minCount;
		protected int maxCount;
		protected string additionalText;
	}
	
	// This class contains a sequence of AssetTypeFileDescs that make
	// up a logical asset type definition
	// AssetTypeFileDesc/AssetTypeDesc are asset type definitions; the
	// asset definition instances are represented in AssetFile and
	// AssetDefinition.
	public class AssetTypeDesc
	{
		protected static string[] assetTypeEnumNames = { 		
			"None",
			"Mesh",
			"Material",
			"SpeedTree",
			"SpeedWind",
			"Plant Type",
			"Particle Script",
			"Sound",
			"Font",
			"Icon",
			"Frame XML",
			"ImageSet",
            "Python Script",
			"Mosaic",
            "Movie",
            "Texture",
			"Other"
		};
		
		public static string AssetTypeEnumName(AssetTypeEnum e)
		{
			return assetTypeEnumNames[(int)e];
		}

        protected static string[] assetTypeEnumFileNames = { 		
            "None",
            "Mesh",
            "Material",
            "SpeedTree",
            "SpeedWind",
            "PlantType",
            "ParticleScript",
            "Sound",
            "Font",
            "Icon",
            "ImageSet",
            "FrameXML",
            "PythonScript",
            "Mosaic",
            "Movie",
            "Texture",
            "Other"
        };
		
		public static string AssetTypeEnumFileName(AssetTypeEnum e)
		{
			return assetTypeEnumFileNames[(int)e];
		}

        public static AssetTypeEnum AssetTypeEnumFromName(string name)
        {
            int ind = Array.IndexOf(assetTypeEnumNames, name);
            return (AssetTypeEnum)(ind >= 0 ? ind : 0);
        }

        //protected static string[] assetStatusEnumNames = {
        //    "None",
        //    "Complete",
        //    "Incomplete",
        //    "Broken"
        //};
		
		public static string AssetStatusEnumName(AssetStatusEnum e)
		{
            //return assetStatusEnumNames[(int)e];
            return e.ToString();
		}
		
		public static AssetStatusEnum AssetStatusEnumFromName(string name)
		{
            //int ind = Array.IndexOf(assetStatusEnumNames, name);
            //return (AssetStatusEnum)(ind >= 0 ? ind : 0);
            return (AssetStatusEnum)Enum.Parse(typeof(AssetStatusEnum), name);
		}

		public AssetTypeDesc(AssetTypeEnum typeEnum, AssetTypeFileDesc[] fileTypes)
		{
			this.typeEnum = typeEnum;
			this.fileTypes = fileTypes;
		}

		protected static AssetTypeDesc[] assetTypes = 
		{
			new AssetTypeDesc(AssetTypeEnum.Mesh,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.Mesh, 1, 1),
							  new AssetTypeFileDesc(AssetFileEnum.AssetDef, 1, int.MaxValue, "Material"),
							  new AssetTypeFileDesc(AssetFileEnum.Physics, 0, 1),
							  new AssetTypeFileDesc(AssetFileEnum.Skeleton, 0, 1)
								  }),
			new AssetTypeDesc(AssetTypeEnum.Material,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.Material, 1, 1),
							  new AssetTypeFileDesc(AssetFileEnum.AssetDef, 0, int.MaxValue),
                              new AssetTypeFileDesc(AssetFileEnum.Texture, 0, int.MaxValue),
							  new AssetTypeFileDesc(AssetFileEnum.Shader, 0, int.MaxValue),
                              new AssetTypeFileDesc(AssetFileEnum.Movie, 0, int.MaxValue)
								  }),
			new AssetTypeDesc(AssetTypeEnum.ParticleScript,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.ParticleScript, 1, 1),
							  new AssetTypeFileDesc(AssetFileEnum.AssetDef, 0, int.MaxValue, "Material"),
								  }),
			new AssetTypeDesc(AssetTypeEnum.FrameXML,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.XML, 1, 1),
							  new AssetTypeFileDesc(AssetFileEnum.PythonScript, 0, int.MaxValue),
							  new AssetTypeFileDesc(AssetFileEnum.XML, 0, int.MaxValue, "ImageSet"),
							  new AssetTypeFileDesc(AssetFileEnum.Texture, 0, int.MaxValue)
								  }),
			new AssetTypeDesc(AssetTypeEnum.ImageSet,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.XML, 1, 1, "ImageSet"),
							  new AssetTypeFileDesc(AssetFileEnum.Texture, 1, int.MaxValue)
								  }),
			new AssetTypeDesc(AssetTypeEnum.SpeedTree,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.SpeedTree, 1, 1),
							  new AssetTypeFileDesc(AssetFileEnum.Texture, 0, int.MaxValue)
								  }),
			new AssetTypeDesc(AssetTypeEnum.SpeedWind,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.SpeedWind, 1, 1)
								  }),
			new AssetTypeDesc(AssetTypeEnum.PlantType,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.ImageSet, 1, 1)
								  }),
			new AssetTypeDesc(AssetTypeEnum.PythonScript,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.PythonScript, 1, 1),
							  new AssetTypeFileDesc(AssetFileEnum.Other, 0, int.MaxValue)
								  }),
			new AssetTypeDesc(AssetTypeEnum.Font,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.Font, 1, 1)
								  }),
			new AssetTypeDesc(AssetTypeEnum.Icon,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.Icon, 1, 1)
								  }),
			new AssetTypeDesc(AssetTypeEnum.Sound,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.Sound, 1, 1)
								  }),
			new AssetTypeDesc(AssetTypeEnum.Mosaic,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.Mosaic, 1, 1),
							  new AssetTypeFileDesc(AssetFileEnum.Texture, 0, int.MaxValue)
								  }),
            new AssetTypeDesc(AssetTypeEnum.Movie,
                          new AssetTypeFileDesc[]
                          {
                              new AssetTypeFileDesc(AssetFileEnum.Movie, 1, int.MaxValue)
                                  }),
            new AssetTypeDesc(AssetTypeEnum.Texture,
                           new AssetTypeFileDesc[]
                            {
                                new AssetTypeFileDesc(AssetFileEnum.Texture, 1 , 1)
                            }),
			new AssetTypeDesc(AssetTypeEnum.Other,
						  new AssetTypeFileDesc[]
						  {
							  new AssetTypeFileDesc(AssetFileEnum.Other, 1, int.MaxValue)
								  })
		};

		public static AssetTypeDesc FindAssetTypeDesc(AssetTypeEnum typeEnum)
		{
			foreach(AssetTypeDesc t in assetTypes) {
				if (t.typeEnum == typeEnum)
					return t;
			}
			return null;
		}
		
		public AssetFileEnum PrimaryFileType
		{
			get { return fileTypes[0].FileTypeEnum; }
		}
	
		public AssetTypeEnum TypeEnum
		{
			get { return typeEnum; }
		}
		
		public AssetTypeFileDesc[] FileTypes
		{
			get { return fileTypes; }
		}
	
		protected AssetTypeEnum typeEnum;
		protected AssetTypeFileDesc[] fileTypes;
	}
	
	// Instances of this class represent one file in a collection of
	// files representing an actual asset.
	public class AssetFile {

		public AssetFile()
		{
			targetFile = "";
			newFileName = "";
            fileTypeEnum = AssetFileEnum.None;
		}
		
		public AssetFile(string targetFile, AssetFileEnum fileTypeEnum)
		{
			this.targetFile = targetFile;
			this.fileTypeEnum = fileTypeEnum;
            newFileName = "";
		}
		
		protected static string[] assetFileEnumNames = {
			"None",
			"Mesh",
			"Material",
			"Skeleton",
			"Texture",
			"Physics",
			"Shader",
			"Particle Script",
			"XML",
			"Python Script",
			"ImageSet",
			"Font",
			"Icon",
			"SpeedTree",
			"SpeedWind",
			"Sound",
			"Asset Definition",
			"Mosaic",
            "Movie",
			"Other"
		};

        protected static string[] compressedAssetFileEnumNames = {
            "None",
            "Mesh",
            "Material",
            "Skeleton",
            "Texture",
            "Physics",
            "Shader",
            "ParticleScript",
            "XML",
            "PythonScript",
            "ImageSet",
            "Font",
            "Icon",
            "SpeedTree",
            "SpeedWind",
            "Sound",
            "AssetDefinition",
            "Mosaic",
            "Movie",
            "Other"
        };
		
		public static string AssetFileEnumName(AssetFileEnum e)
		{
			return assetFileEnumNames[(int)e];
		}
		
		public static AssetFileEnum AssetFileEnumFromName(string name)
		{
			int ind = Array.IndexOf(assetFileEnumNames, name);
			return (AssetFileEnum)(ind >= 0 ? ind : 0);
		}

		protected static string CompressedFileEnumName(AssetFileEnum e)
		{
			return compressedAssetFileEnumNames[(int)e];
            //return e.ToString();
		}
		
		public static AssetFileEnum AssetFileEnumFromCompressedName(string name)
		{
			int ind = Array.IndexOf(compressedAssetFileEnumNames, name);
			return (AssetFileEnum)(ind >= 0 ? ind : 0);
            //return (AssetFileEnum) Enum.Parse(typeof(AssetFileEnum), name);
		}

		protected static string[] assetFileStatusNames = {
			"None",
			"InRepository",
			"AddedToRepository"
		};

		public static string AssetFileStatusName(AssetFileStatus e)
		{
			//return assetFileStatusNames[(int)e];
            return e.ToString();
		}
		
		public static AssetFileStatus AssetFileStatusFromName(string name)
		{
            //int ind = Array.IndexOf(assetFileStatusNames, name);
            //return (AssetFileStatus)(ind >= 0 ? ind : 0);
            return (AssetFileStatus) Enum.Parse(typeof(AssetFileStatus), name);
		}

		protected static ExtensionMapping[] extensionMappings = 
		{
			new ExtensionMapping(AssetTypeEnum.FrameXML, true, ".xml", "Interface\\FrameXML", AssetFileEnum.XML),
			new ExtensionMapping(AssetTypeEnum.FrameXML, false, ".xml", "Interface\\Imagesets", AssetFileEnum.XML),
			new ExtensionMapping(AssetTypeEnum.FrameXML, ".py", "Interface\\FrameXML", AssetFileEnum.PythonScript),
			new ExtensionMapping(AssetTypeEnum.FrameXML, ".dds", "ImageFiles", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.FrameXML, ".tga", "ImageFiles", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.FrameXML, ".jpg", "ImageFiles", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.FrameXML, ".bmp", "ImageFiles", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.FrameXML, ".png", "ImageFiles", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.PythonScript, ".py", "Scripts", AssetFileEnum.PythonScript),
            // AssetTypeEnum.None means that it applies to all remaining asset types
			new ExtensionMapping(AssetTypeEnum.None, ".mesh", "Meshes", AssetFileEnum.Mesh),
			new ExtensionMapping(AssetTypeEnum.None, ".material", "Materials", AssetFileEnum.Material),
			new ExtensionMapping(AssetTypeEnum.None, ".skeleton", "Skeletons", AssetFileEnum.Skeleton),
			new ExtensionMapping(AssetTypeEnum.None, ".cg", "GpuPrograms", AssetFileEnum.Shader),
			new ExtensionMapping(AssetTypeEnum.None, ".program", "GpuPrograms", AssetFileEnum.Shader),
			new ExtensionMapping(AssetTypeEnum.None, ".dds", "Textures", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.None, ".tga", "Textures", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.None, ".jpg", "Textures", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.None, ".bmp", "Textures", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.None, ".png", "Textures", AssetFileEnum.Texture),
			new ExtensionMapping(AssetTypeEnum.None, ".particle", "Particles", AssetFileEnum.ParticleScript),			
			new ExtensionMapping(AssetTypeEnum.None, ".physics", "Physics", AssetFileEnum.Physics),
			new ExtensionMapping(AssetTypeEnum.None, ".imageset", "ImageSets", AssetFileEnum.ImageSet),
			new ExtensionMapping(AssetTypeEnum.None, ".xml", "Interface\\FrameXML", AssetFileEnum.XML),
			new ExtensionMapping(AssetTypeEnum.None, ".tre", "SpeedTree", AssetFileEnum.SpeedTree),
			new ExtensionMapping(AssetTypeEnum.None, ".ini", "SpeedTree", AssetFileEnum.SpeedWind),
			new ExtensionMapping(AssetTypeEnum.None, ".wav", "Sounds", AssetFileEnum.Sound),
			new ExtensionMapping(AssetTypeEnum.None, ".ogg", "Sounds", AssetFileEnum.Sound),
			new ExtensionMapping(AssetTypeEnum.None, ".ttf", "Fonts", AssetFileEnum.Font),
			new ExtensionMapping(AssetTypeEnum.None, ".ico", "Icons", AssetFileEnum.Icon),
			new ExtensionMapping(AssetTypeEnum.None, ".py", "Interface\\FrameXML", AssetFileEnum.PythonScript),
			new ExtensionMapping(AssetTypeEnum.None, ".asset", "AssetDefinitions", AssetFileEnum.AssetDef),
			new ExtensionMapping(AssetTypeEnum.None, ".mmf", "Textures", AssetFileEnum.Mosaic),
            new ExtensionMapping(AssetTypeEnum.None, ".mpg", "Movies", AssetFileEnum.Movie),
            new ExtensionMapping(AssetTypeEnum.None, ".wmv", "Movies", AssetFileEnum.Movie),
            new ExtensionMapping(AssetTypeEnum.None, ".wma", "Movies", AssetFileEnum.Movie),
            new ExtensionMapping(AssetTypeEnum.None, ".asf", "Movies", AssetFileEnum.Movie),
            new ExtensionMapping(AssetTypeEnum.None, ".mp3", "Movies", AssetFileEnum.Movie),
			new ExtensionMapping(AssetTypeEnum.None, "", "Misc", AssetFileEnum.None)
		};
		
		public static string DirectoryForFileEnum(AssetTypeEnum type, bool primaryFile, 
												  AssetFileEnum fileTypeEnum)
		{
            foreach (ExtensionMapping mapping in extensionMappings) {
				if ((fileTypeEnum == mapping.FileTypeEnum) &&
					(type == mapping.AssetType || mapping.AssetType == AssetTypeEnum.None) &&
					(mapping.PrimaryMatch == ExtensionPrimaryEnum.DontCare || 
					 (mapping.PrimaryMatch == ExtensionPrimaryEnum.MatchPrimary) == primaryFile))
					return mapping.AssetDirectory;
			}
			return "Misc";
		}
		
		public static string DirectoryForFile(AssetTypeEnum type, bool primaryFile, string fileName)
		{
			string extension = Path.GetExtension(fileName);
			AssetFileEnum fileTypeEnum = EnumForExtension(extension);
			return DirectoryForFileEnum(type, primaryFile, fileTypeEnum);
		}

		public static string ExtensionForEnum(AssetFileEnum fileTypeEnum)
		{
			foreach (ExtensionMapping mapping in extensionMappings) {
				if (mapping.FileTypeEnum == fileTypeEnum)
					return mapping.FileExtension;
			}
			return "";
		}

		public static bool KnownFileExtension(string extension)
		{
			foreach (ExtensionMapping mapping in extensionMappings) {
				string mapExt = mapping.FileExtension;
				if (mapExt != "" && mapExt == extension)
					return true;
			}
			return false;
		}
		
		// Returns all extensions
		public static List<string> AllExtensionsForEnum(AssetFileEnum fileTypeEnum)
		{
            List<string> strings = new List<string>();
			foreach (ExtensionMapping mapping in extensionMappings) {
				if (mapping.AssetType == AssetTypeEnum.None && mapping.FileTypeEnum == fileTypeEnum)
					strings.Add(mapping.FileExtension);
			}
			return strings;
		}

		public static AssetFileEnum EnumForExtension(string extension)
		{
			foreach (ExtensionMapping mapping in extensionMappings) {
				if (mapping.FileExtension == extension)
					return mapping.FileTypeEnum;
			}
			return AssetFileEnum.None;
		}

		// Xml Machinery
		
		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("AssetFile");
			w.WriteElementString("TargetFile", targetFile);
			w.WriteElementString("NewFileName", newFileName);
			w.WriteElementString("FileType", CompressedFileEnumName(fileTypeEnum));
			w.WriteEndElement();
		}
		
		public static AssetFile FromXml(XmlReader r, List<string> log)
		{
			AssetFile file = new AssetFile();
			file.FromXmlInternal(r, log);
			return file;
		}
		
		protected void FromXmlInternal(XmlReader r, List<string> log)
		{
			while(r.Read())
            {
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
				switch(r.Name)
                {
                    case "TargetFile":
                        targetFile = r.ReadElementString();
                        break;
                    case "NewFileName":
                        newFileName = r.ReadElementString();
                        break;
                    case "FileType":
                        fileTypeEnum = AssetFileEnumFromCompressedName(r.ReadElementString());
                        break;
                    default:
                        log.Add(string.Format("In AssetFile.FromXmlInternal, unknown attribute '{0}'", r.Name));
                        break;
				}
			}
		}
		
		// AssetFile Properties

		public string TargetFile 
		{
			get { return targetFile; }
			set { targetFile = value; }
		}
			
		public string NewFileName
		{
			get { return newFileName; }
			set { newFileName = value; }
		}
			
		public AssetFileEnum FileTypeEnum 
		{
			get { return fileTypeEnum; }
			set { fileTypeEnum = value; }
		}
			
		public string FileExtension
		{
			get { return AssetFile.ExtensionForEnum(fileTypeEnum); }
		}

		protected string targetFile;
		protected string newFileName;
		protected AssetFileEnum fileTypeEnum;
	}

	public class AssetProperty
	{
		public AssetProperty()
		{
			this.name = "";
			this.propValue = "";
		}
		
		
		public AssetProperty(string name, string propValue)
		{
			this.name = name;
			this.propValue = propValue;
		}

        public AssetProperty(AssetProperty asset)
        {
            this.name = asset.Name;
            this.propValue = asset.Value;
        }
		
		protected string name;
		protected string propValue;

		public string Name 
		{
			get { return name; }
			set { name = value; }
		}

		public string Value 
		{
			get { return propValue; }
			set { propValue = value; }
		}
			
		public static AssetProperty FromXml(XmlReader r)
		{
			AssetProperty property = new AssetProperty();
			property.FromXmlInternal(r);
			return property;
		}
		
		protected void FromXmlInternal(XmlReader r)
		{
			for(int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
				switch (r.Name)
                {
                    case "name":
                        name = r.Value;
                        break;
				    case "value":
                        propValue = r.Value;
                        break;
				}
			}
            r.MoveToElement();
		}
	
		public void ToXml(XmlWriter w)
		{
			//w.WriteElementWithAttributes("Property", string.Format("name=\"{0}\" value=\"{1}\"", name, propValue));
            w.WriteStartElement("Property");
            w.WriteAttributeString("name", name);
            w.WriteAttributeString("value", propValue);
            w.WriteEndElement();
		}
		
	}
	
	// An asset definition, consisting of some metadata
	// (name/description/category/status) for an asset, together with
	// the list of AssetFiles that make up the asset
	public class AssetDefinition
	{
		public AssetDefinition()
		{
			name = "";
            description = "";
			category = "";
			typeEnum = AssetTypeEnum.None;
			status = AssetStatusEnum.Incomplete;
            breakage = "";
			files = new List<AssetFile>();
			properties = new List<AssetProperty>();
		}
		
		public AssetDefinition(string name, AssetTypeEnum typeEnum, AssetFile file)
		{
			this.name = name;
            description = "";
			category = "";
			this.typeEnum = typeEnum;
			status = AssetStatusEnum.Complete;
            breakage = "";
			List<AssetFile> files = new List<AssetFile>();
			files.Add(file);
			this.files = files;
			properties = new List<AssetProperty>();
		}
		
		public AssetFile PrimaryFile
		{
			get {
                if (files.Count > 0)
				return files[0];
			else
				return null;
            }
		}
		
		public bool Complete()
		{
			return status == AssetStatusEnum.Complete;
		}
		
		public AssetStatusEnum ComputeStatus()
		{
			AssetTypeDesc typeDesc = AssetTypeDesc.FindAssetTypeDesc(typeEnum);
			if (files.Count == 0 || typeDesc == null) {
				status = AssetStatusEnum.Incomplete;
				breakage = "No files in asset definition";
				return status;
			}
			AssetTypeDesc type = AssetTypeDesc.FindAssetTypeDesc(typeEnum);
			AssetFile primaryFile = files[0];
			if (primaryFile.FileTypeEnum != type.PrimaryFileType) {
				status = AssetStatusEnum.Broken;
				breakage = "Primary file is not of the right type";
				return status;
			}
			// Now go through the other types, asking if we have the
			// right numbers of them
			int countRemaining = files.Count - 1;
			int descCount = 0;
			foreach(AssetTypeFileDesc desc in type.FileTypes) {
				descCount++;
				if (descCount == 1)
					continue;    // We've already checked the primary file
				int cnt = CountFilesOfType(desc.FileTypeEnum);
				if (cnt < desc.MinCount) {
					status = AssetStatusEnum.Incomplete;
					breakage = "Not enough files of type " + desc.FileExtension;
					return status;
				}
				if (cnt > desc.MaxCount) {
					status = AssetStatusEnum.Incomplete;
					breakage = "Too many files of type " + desc.FileExtension;
					return status ;
				}
				countRemaining -= cnt;
			}
			if (countRemaining > 0 && typeEnum != AssetTypeEnum.Other) {
				status = AssetStatusEnum.Broken;
				breakage = "There are files of types not allowed for this asset type";
				return status;
			}
			status = AssetStatusEnum.Complete;
			breakage = "";
            return status;
		}
		
		public int CountFilesOfType(AssetFileEnum fileTypeEnum)
		{
			int cnt = 0;
			foreach(AssetFile desc in files) {
				if (desc.FileTypeEnum == fileTypeEnum)
					cnt++;
			}
			return cnt;
		}
		
        public void SetDescriptionFromName()
		{
			int index = name.LastIndexOf("_");
			if (index >= 0)
				description = name.Substring(0, index);
		}
		
		public void ToXml(XmlWriter w) 
		{
			w.WriteStartElement("AssetDefinition");
            if (name.Length != 0)
            {
                w.WriteElementString("Name", name);
            }
            if (description.Length != 0)
            {
                w.WriteElementString("Description", description);
            }
            if (category.Length != 0)
            {
                w.WriteElementString("Category", category);
            }
            if (typeEnum != 0)
            {
                w.WriteElementString("Type", AssetTypeDesc.AssetTypeEnumName(typeEnum));
            }
			if (status != AssetStatusEnum.Complete)
                w.WriteElementString("Status", AssetTypeDesc.AssetStatusEnumName(status));
            if (breakage.Length != 0)
            {
                w.WriteElementString("Breakage", breakage);
            }
			w.WriteStartElement("Files");
			foreach (AssetFile file in files)
				file.ToXml(w);
			w.WriteEndElement();  //Files
			if (properties.Count > 0) {
				w.WriteStartElement("Properties");
				foreach (AssetProperty property in properties)
					property.ToXml(w);
				w.WriteEndElement();//Properties
			}
			w.WriteEndElement(); //AssetDefinition
			return;
		}

		public void WriteXmlFile(string path)
		{
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
			XmlWriter w = XmlWriter.Create(path, xmlSettings);
			ToXml(w);
			w.Close();
		}
		
		public static AssetDefinition ReadXmlFile(string path, List<string> log)
		{
			AssetDefinition assetDefinition = new AssetDefinition();
            XmlReader r = XmlReader.Create(path, new XmlReaderSettings());
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.XmlDeclaration)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                switch (r.Name)
                {
                    case "AssetDefinition":
                        {
                            if (assetDefinition.ReadXmlFileInternal(r, log))
                            {
                                return assetDefinition;
                            }
                            else
                            {
                                return null;
                            }
                        }
                }
            }
            return null;
		}
		
		protected bool ReadXmlFileInternal(XmlReader r, List<string> log)
		{
            //Debug.Assert(root.LocalName == "AssetDefinition");
            while(r.Read()) {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                switch (r.Name)
                {
                    case "Name":
                        name = r.ReadElementString();
                        break;
                    case "Description":
                        description = r.ReadElementString();
                        break;
                    case "Category":
                        category = r.ReadElementString();
                        break;
                    case "Type":
                        typeEnum = AssetTypeDesc.AssetTypeEnumFromName(r.ReadElementString());
                        break;
                    case "Status":
                        status = AssetTypeDesc.AssetStatusEnumFromName(r.ReadElementString());
                        break;
                    case "Breakage":
                        breakage = r.ReadElementString();
                        break;
                    case "Files":

                        while (r.Read())
                        {
                            if (r.NodeType == XmlNodeType.Whitespace)
                            {
                                continue;
                            }
                            if (r.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                            switch (r.Name)
                            {
                                case "AssetFile":
                                    files.Add(AssetFile.FromXml(r, log));

                                    break;
                            }
                        }
                        break;
                    case "Properties":
                        while (r.Read())
                        {
                            if (r.NodeType == XmlNodeType.Whitespace)
                            {
                                continue;
                            }
                            if (r.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                            switch (r.Name)
                            {
                                case "Property":
                                    properties.Add(AssetProperty.FromXml(r));
                                    break;
                            }
                        }
                        break;

                    default:
                        log.Add(string.Format("In AssetDefinition.ReadXmlFileInternal, unknown attribute '{0}'",
                            r.Name));
                        break;
                }
            }
            return true;
			
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		public string Category
		{
			get { return category; }
			set { category = value; }
		}

		public AssetTypeEnum TypeEnum
		{
			get { return typeEnum; }
			set { typeEnum = value; }
		}

		public AssetStatusEnum Status
		{
			get { return status; }
			set { status = value; }
		}

		public string Breakage
		{
			get { return breakage; }
			set { breakage = value; }
		}

		public List<AssetFile> Files
		{
			get { return files; }
			set { files = value; }
		}

		public List<AssetProperty> Properties
		{
			get { return properties; }
			set { properties = value; }
		}
		
		// The state represented by an asset definition
		protected string name;
        protected string description;
        protected string category;
		protected AssetTypeEnum typeEnum;
		protected List<AssetFile> files;
		protected List<AssetProperty> properties;
		protected AssetStatusEnum status;
		protected string breakage;
	}
	
	// This class contains functionality for extracting asset
	// dependencies from Axiom script files.  It doesn't do an actual
	// parse of the scripts; just enough to get the dependent stuff.
	public class ScriptParser
	{
		public static void AddIfNotPresent(List<string> strings, string value)
		{
			if (strings.IndexOf(value) < 0)
				strings.Add(value);
		}

        public static string MakeFullFilePath(string dir, string file)
        {
            return dir + "\\" + file;
        }

        protected static string[] cubicPostfixes = { "_fr", "_bk", "_lf", "_rt", "_up", "_dn" };

        public static Dictionary<string, List<string>> ParseScript(string path, string commentToken, 
																   string breakCharacters,
																   string[] introducers,
																   string overlordString, string overloadIntroducer)
		{
			Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
			foreach (string introducer in introducers)
				dict.Add(introducer, new List<string>());
			if (overlordString.Length > 0 && !dict.ContainsKey(overloadIntroducer))
				dict.Add(overloadIntroducer, new List<string>());
			FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
			StreamReader reader = new StreamReader(f);
			char [] delimeters = breakCharacters.ToCharArray();
			string line;
			while ((line = reader.ReadLine()) != null) {
				string[] strings = line.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);
				if (overlordString.Length > 0 && strings.Length > 2 && 
					strings[0] == overlordString) {
					List<string> capturedStrings = dict[overloadIntroducer];
					if (strings.Length == 3 && strings[2] == "combinedUVW") {
						string root = strings[1];
						foreach (string s in cubicPostfixes)
							AddIfNotPresent(capturedStrings, 
											Path.GetFileNameWithoutExtension(root) + s + Path.GetExtension(root));
					}
					else // Add the second through the next-to-last ones
						for (int i = 1; i < strings.Length - 1; i++) {
							AddIfNotPresent(capturedStrings, strings[i]);
					}
					continue;
				}
				for (int i=0; i<strings.Length - 1; i++) {
					string s = strings[i].ToLower();
					// If we see the comment token, stop parsing the line
					if (s.StartsWith(commentToken))
						break;
					// Iterate over the introducers; if we get a
					// match, see if we already have the value; if
					// not, put it in the set.
					foreach (string introducer in introducers) {
						if (s == introducer) {
							string value = strings[i+1];
							List<string> capturedStrings = dict[introducer];
							AddIfNotPresent(capturedStrings, value);
							break;
						}
					}
				}
			}
			reader.Close();
			return dict;
		}
	
		public static List<string> BreakLineIntoTokens(string line, string whitespace, 
                                                       string singleCharTokens, string commentToken)
		{
			List<string> strings = new List<string>();
			int tokenStart = 0;
			bool inToken = false;
			// Add a space at the end, so we know we won't be in a
			// token when we exit the loop
			line += whitespace.Substring(0, 1);
			for (int i=0; i<line.Length; i++) {
				char ch = line[i];
				int w = whitespace.IndexOf(ch);
				int s = singleCharTokens.IndexOf(ch);
                bool comment = ch == commentToken[0] && line.Length - i >= commentToken.Length &&
                     line.Substring(i, commentToken.Length) == commentToken;
				if (comment || w >= 0 || s >= 0) {
					if (inToken) {
						strings.Add(line.Substring(tokenStart, i - tokenStart));
						inToken = false;
					}
                    if (comment)
                        break;
                    if (s >= 0)
                        strings.Add(new string(ch, 1));
					continue;
				}
				else if (!inToken) {
					inToken = true;
					tokenStart = i;
				}
			}
            return strings;
		}
		
		public static List<string> ExtractParticleSystemNamesFromScript(string path)
		{
			List<string> particleSystems = new List<string>();
			FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
			StreamReader reader = new StreamReader(f);
			int nesting = 0;
			string line;
			while ((line = reader.ReadLine()) != null) {
				List<string> strings = BreakLineIntoTokens(line, " \t", "{}", "//");
				foreach (string s in strings) {
					if (s == "{")
						nesting++;
					else if (s == "}")
						nesting--;
					else if (nesting == 0)
						particleSystems.Add(s);
				}
			}
			return particleSystems;
		}
		
		public static List<string> ExtractShaderNamesFromScript(string path)
		{
			List<string> shaderNames = new List<string>();
			FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
			StreamReader reader = new StreamReader(f);
			int nesting = 0;
			string previousToken = "";
			string line;
			while ((line = reader.ReadLine()) != null) {
				List<string> strings = BreakLineIntoTokens(line, " \t", "{}():;,", "//");
				foreach (string s in strings) {
					if (nesting == 0 && s == "(")
						shaderNames.Add(previousToken);
					if (s == "{" || s == "(")
						nesting++;
					else if (s == "}" || s == ")")
						nesting--;
					previousToken = s;
				}
			}
			return shaderNames;
		}
		
		public static List<string> ExtractShaderNamesAndFiles(string path, bool material, out List<string> files)
		{
			string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.Material, false, AssetFileEnum.Shader);
			List<string> shaderNames = new List<string>();
			files = new List<string>();
			FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
			StreamReader reader = new StreamReader(f);
			int nesting = 0;
			string previousToken = "";
			string line;
            string name = "";
			string source = "";
            string entry_point = "";
			while ((line = reader.ReadLine()) != null) {
				List<string> strings = BreakLineIntoTokens(line, " \t", "{}", "//");
				foreach (string s in strings) {
					if (nesting == 0 && 
						(previousToken == "vertex_program" || previousToken == "fragment_program")) {
						name = s;
						source = "";
						entry_point = "";
					}
					else if (previousToken == "source")
						source = s;
					else if (previousToken == "entry_point") {
						entry_point = s;
						if (!material) {
							files.Add(MakeFullFilePath(dir, Path.GetFileName(path)));
							shaderNames.Add(name);
						}
						files.Add(MakeFullFilePath(dir, source));
						shaderNames.Add(name);
					}
					previousToken = s;
				}
			}
			return shaderNames;
		}
		
		public static Dictionary<string, List<string>> ParseMaterialScript(string path)
		{
			string[] materialIntroducers = new string[] { "material", "texture", "filename",
														  "vertex_program_ref", "fragment_program_ref" };
			return ParseScript(path, "//", " \t{}", materialIntroducers, "cubic_texture", "texture");
		}

		public static Dictionary<string, List<string>> ParseParticleScript(string path)
		{
			string[] particleIntroducers = new string[] { "material" };
			return ParseScript(path, "//", " \t{}", particleIntroducers, "", "");
		}
		
	}
	
	// This is a class that bundles together the fileName with the
	// full pathname of an asset file.  I'm not at all sure this is
	// really required.
	public class AssetFileInfo
	{
		public string FileName;
		public string FullName;
		public AssetFileInfo(string FileName, string FullName)
		{
			this.FileName = FileName;
			this.FullName = FullName;
		}
	}
	
	// The public asset repository class.
	public class RepositoryClass
    {
        // The list of asset directories in a repository.  This is
        // canned and predetermined; AssetConfig.xml and
        // EngineConfig.xml are neither required nor permitted.
		public static string[] repositoryDirectories = {
				"AssetDefinitions",
				"Fonts",
				"GpuPrograms",
				"Icons",
				"Imagefiles",
				"Interface\\FrameXML",
				"Interface\\Imagesets",
				"Materials",
				"Meshes",
				"Misc",
                "Movies",
				"Particles",
				"Physics",
				"Scripts",
				"Skeletons",
				"Sounds",
				"SpeedTree",
				"Textures"
					};
					
		public static string[] axiomDirectories = {
				"AssetDefinitions",
				"GpuPrograms",
				"Icons",
				"Imagefiles",
				"Materials",
				"Meshes",
				"Misc",
                "Movies",
				"Particles",
				"Physics",
				"Skeletons",
				"Sounds",
				"SpeedTree",
				"Textures"
					};
		
		protected static string[] clientResources = {
			"Font", "Fonts",
			"Imageset", "Interface\\ImageSets",
			"Interface", "Interface\\FrameXML",
			"Script", "Interface\\FrameXML",
			"Script", "Scripts" };
		
		protected static string repositoryKey = Registry.CurrentUser + "\\Software\\Multiverse\\AssetRepository";
        // This is only used for legacy registry keys - - all new get APIs
        // only access this key if the repositoryPathListAttrName key
        // does not exist, and all sets set the
        // repositoryPathListAttrName key.
        protected static string repositoryPathAttrName = "RepositoryDirectory";
        protected static string repositoryPathListAttrName = "RepositoryDirectories";

        public List<string> CheckForValidRepository()
        {
            return CheckForValidRepository(repositoryDirectoryList);
        }
		
		public List<string> CheckForValidRepository(List<string> directoryList)
		{
			// bool good = true;
			List<string> log = new List<string>();
			if (directoryList == null || directoryList.Count == 0) {
				log.Add("The repository directory list contains no directories");
				return log;
			}
			foreach (string axiomDir in axiomDirectories) {
				if (!RequiredDirectoryExists(directoryList, axiomDir)) {
					log.Add(string.Format("Directory '{0}' does not exist in any of the repositories", axiomDir));
					// good = false;
				}
			}
			for (int i=0; i<clientResources.Length; i+=2) {
				if (!RequiredDirectoryExists(directoryList, clientResources[i+1])) {
					log.Add(string.Format("Directory '{0}' does not exist in any of the repositories", clientResources[i+1]));
					// good = false;
				}
			}
			return log;
		}

        protected bool RequiredDirectoryExists(List<string> directoryList, string dirTail) {
            foreach (string dir in directoryList) {
                if (Directory.Exists(dir + "\\" + dirTail))
                    return true;
            }
            return false;
        }
        
        public string GetRepositoryDirectoriesString() 
        {
            List<string> directories = GetRepositoryDirectoriesFromRegistry();
            if (directories == null && directories.Count == 0)
                return "";
            else {
                string s = "";
                foreach (string dir in directories) {
                    if (s != "")
                        s += "; ";
                    s += dir;
                }
                return s;
            }
		}
            
        
        public void SetRepositoryDirectoriesInRegistry(List<string> directories) {
            Registry.SetValue(repositoryKey, repositoryPathListAttrName, directories.ToArray());
            repositoryDirectoryList = directories;
        }

        protected List<string> GetRepositoryDirectoriesFromRegistry() {
            repositoryDirectoryList = new List<string>();
            Object value = Registry.GetValue(repositoryKey, repositoryPathListAttrName, null);
            if (value != null) {
                string[] paths = value as string[];
                if (paths.Length > 0 && paths[0] != string.Empty)
                    repositoryDirectoryList = new List<string>(paths);
            }  else {
                value = Registry.GetValue(repositoryKey, repositoryPathAttrName, null);
                if (value != null)
                    repositoryDirectoryList.Add((string)value);    
            }
            return repositoryDirectoryList;
        }

        public bool RepositoryDirectoryListSet() {
            return repositoryDirectoryList != null && repositoryDirectoryList.Count > 0;
        }

        public bool DifferentDirectoryList(List<string> directories) {
            if (repositoryDirectoryList == null)
                return true;
            int count = repositoryDirectoryList.Count;
            if (count != directories.Count)
                return true;
            for(int i=0; i<count; i++)
                if (directories[i] != repositoryDirectoryList[i])
                    return true;
            return false;
        }
        
        public static string MakeFullFilePath(string dir, string fileName) {
            return Path.Combine(dir, fileName);
        }
		
		public string MakeRepositoryFilePath(string fileName)
		{
			return MakeFullFilePath(LastRepositoryPath, fileName);
		}
		
		// This method reads all the directories in Media, putting all
		// the file names in assetFiles, sorted by AssetFileEnum, so
		// we can quickly find out if a given file is already in the
		// repository.
		void ReadAllAssetPaths(List<string> directories, string[] subdirectories, List<string> log)
		{
			assetFiles = new Dictionary<AssetFileEnum, Dictionary<string, AssetFileInfo>>();
			for (AssetFileEnum i = AssetFileEnum.Mesh; i<=AssetFileEnum.Other; i++) {
				assetFiles.Add(i, new Dictionary<string, AssetFileInfo>());
			}
            foreach (string directory in directories) {
                foreach(string subdirectory in subdirectories) {
                    string dir = MakeFullFilePath(directory, subdirectory);
                    if (!Directory.Exists(dir))
                        continue;
                    DirectoryInfo info = new DirectoryInfo(dir);
                    FileInfo[] files = info.GetFiles();
                    foreach (FileInfo file in files) {
                        string extension = file.Extension.ToLower();
                        if (!AssetFile.KnownFileExtension(extension))
                            continue;
                        AssetFileEnum type = AssetFile.EnumForExtension(extension);
                        Dictionary<string, AssetFileInfo> filesOfType = assetFiles[type];
                        string filenameSansPrefix = Path.Combine(subdirectory, file.Name);
                        string filename = Path.Combine(directory, filenameSansPrefix);
                        AssetFileInfo both;
                        // Only put the first instance in filesOfType
                        if (!filesOfType.TryGetValue(filenameSansPrefix, out both))
                            filesOfType.Add(filenameSansPrefix, new AssetFileInfo(filename, file.FullName));
                    }
                }
			}
		}
		
		void ReadAllAssetDefinitions(List<string> log)
		{
			assetDefinitions = new Dictionary<string, AssetDefinition>();
			Dictionary<string, AssetFileInfo> defFiles = assetFiles[AssetFileEnum.AssetDef];
			foreach(AssetFileInfo info in defFiles.Values) {
				AssetDefinition def = AssetDefinition.ReadXmlFile(info.FullName, log);
				if (def != null) {
                    AssetDefinition previousDef = null;
                    if (assetDefinitions.TryGetValue(def.Name, out previousDef))
                        log.Add(string.Format("An asset definition for '{0}', in file '{1}', already exists, and assets with duplicate names are not permitted!",
                                def.Name, info.FullName));
					else
                        assetDefinitions.Add(def.Name, def);
                }
			}
		}

		public void ReadAllMaterialFiles (List<string> log)
		{
			materialNameToMaterialFileName = new Dictionary<string, string>();
			materialFileNameToTextureNames = new Dictionary<string, List<string>>();
            materialFileNameToMovieNames = new Dictionary<string, List<string>>();
            materialFileNameToMaterialNames = new Dictionary<string, List<string>>();
			Dictionary<string, AssetFileInfo> materialFiles = assetFiles[AssetFileEnum.Material];
			List<string> temp;
			foreach (AssetFileInfo info in materialFiles.Values) {
				List<string> referencedFiles;
				List<string> shaderNames = ScriptParser.ExtractShaderNamesAndFiles(info.FullName, true, out referencedFiles);
				for (int i=0; i<shaderNames.Count; i++) {
					string name = shaderNames[i];
					AddToShaderNameToFileMapping(name, referencedFiles[i]);
				}
				Dictionary<string, List<string>> results = ScriptParser.ParseMaterialScript(info.FullName);
				foreach (string materialName in results["material"]) {
					if (materialNameToMaterialFileName.ContainsKey(materialName))
                        log.Add(string.Format("Material name '{0}' is defined in both '{1}' and '{2}'",
                                              materialName, materialNameToMaterialFileName[materialName],
                                              info.FileName));
                    else {
                        materialNameToMaterialFileName.Add(materialName, info.FileName);
					    if (!materialFileNameToMaterialNames.TryGetValue(info.FileName, out temp)) {
						    temp = new List<String>();
						    materialFileNameToMaterialNames.Add(info.FileName, temp);
					    }
					    temp.Add(materialName);
                    }
				}
				foreach (string textureName in results["texture"]) {
					if (!materialFileNameToTextureNames.TryGetValue(info.FileName, out temp)) {
						temp = new List<String>();
						materialFileNameToTextureNames.Add(info.FileName, temp);
					}
					temp.Add(textureName);
				}
                foreach (string movieName in results["filename"])
                {
                    if (!materialFileNameToMovieNames.TryGetValue(info.FileName, out temp))
                    {
                        temp = new List<String>();
                        materialFileNameToMovieNames.Add(info.FileName, temp);
                    }
                    temp.Add(movieName);
                }
			}
		}
		
		public void ReadAllParticleScripts (List<string> log)
		{
            particleSystemToParticleSystemFileName = new Dictionary<string,string>();
            particleScriptFileNameToParticleSystemNames = new Dictionary<string, List<string>>();
			particleScriptFileNameToTextureNames = new Dictionary<string, List<string>>();
			particleScriptFileNameToMaterialNames = new Dictionary<string, List<string>>();
			Dictionary<string, AssetFileInfo> particleFiles = assetFiles[AssetFileEnum.ParticleScript];
			List<string> temp;
			string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.ParticleScript, false, AssetFileEnum.ParticleScript);
			foreach (AssetFileInfo info in particleFiles.Values) {
				List<string> systems = ScriptParser.ExtractParticleSystemNamesFromScript(info.FullName);
                string fileName = MakeFullFilePath(dir, Path.GetFileName(info.FullName));
				particleScriptFileNameToParticleSystemNames.Add(fileName, systems);
                foreach (string system in systems)
				    particleSystemToParticleSystemFileName.Add(system, fileName);
				Dictionary<string, List<string>> results = ScriptParser.ParseParticleScript(info.FullName);
				foreach (string materialName in results["material"]) {
					if (!particleScriptFileNameToMaterialNames.TryGetValue(fileName, out temp)) {
						temp = new List<String>();
						particleScriptFileNameToMaterialNames.Add(fileName, temp);
					}
					temp.Add(materialName);
				}
			}
		}

        //private string MakeFullFilePath(string dir, string p) {
        //    throw new Exception("The method or operation is not implemented.");
        //}

		protected void AddToShaderNameToFileMapping(string name, string path)
		{
			List<string> shaderPathnames;
			if (!shaderNameToShaderFileNames.TryGetValue(name, out shaderPathnames)) {
				shaderPathnames = new List<string>();
				shaderNameToShaderFileNames.Add(name, shaderPathnames);
			}
			ScriptParser.AddIfNotPresent(shaderPathnames, path);
		}
		
		protected void ReadAllGpuPrograms(List<string> log)
		{
			string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.Material, false, AssetFileEnum.Shader);
            shaderNameToShaderFileNames = new Dictionary<string, List<string>>();
			Dictionary<string, AssetFileInfo> shaderFiles = assetFiles[AssetFileEnum.Shader];
			// List<string> temp;
			foreach (AssetFileInfo info in shaderFiles.Values) {
				List<string> shaderNames;
				if (Path.GetExtension(info.FileName).ToLower() == ".program") {
					List <string> referencedFiles;
					shaderNames = ScriptParser.ExtractShaderNamesAndFiles(info.FullName, false, out referencedFiles);
					for (int i=0; i<shaderNames.Count; i++) {
                        string name = shaderNames[i];
                        AddToShaderNameToFileMapping(name, MakeFullFilePath(dir, Path.GetFileName(info.FullName)));
						AddToShaderNameToFileMapping(name, referencedFiles[i]);
					}
				}
				else {
					shaderNames = ScriptParser.ExtractShaderNamesFromScript(info.FullName);
					foreach (string name in shaderNames)
						{
							if (name == "expand" || name == "compress")  // Hack, hack, hack
								continue;
							AddToShaderNameToFileMapping(name, info.FullName);
						}
				}
			}
		}

        /// <summary>
        ///   Initialize the repository
        /// </summary>
        /// <param name="directories">the base repository paths</param>
        /// <returns>A list containing any error messages</returns>
        public List<string> InitializeRepository(List<string> directories)
		{
            InitializeRepositoryPath(directories);
			List<string> log = CheckForValidRepository();
			if (log.Count > 0)
				return log;
			ReadAllAssetPaths(directories, repositoryDirectories, log);
			ReadAllAssetDefinitions(log);
			ReadAllGpuPrograms(log);
			ReadAllMaterialFiles(log);
			ReadAllParticleScripts(log);
			ComputeAssetsWithPrimaryFileName(log);
			return log;
		}
		
//         public void ReadMeshMaterialAndSkeleton(string meshFile,
// 												out string materialName,
// 												out string skeletonName)
//         {
//             MeshSerializer meshReader = new MeshSerializer();
//             Stream data = new FileStream(meshFile, FileMode.Open);
//             // import the .mesh file
//             DependencyInfo info = meshReader.GetMaterialAndSkeletonNames(data);
//             materialName = info.materials[0];
//             skeletonName = info.skeletons[0]; 
//         }
		
		public static List<string> ReadStreamLines(StreamReader reader) 
        {
			List<string> lines = new List<string>();
			string line;
			while ((line = reader.ReadLine()) != null) {
                line = line.Trim();
				if (line.Length > 0)
					lines.Add(line);
			}
			return lines;
		}

        // Read all the lines in the text file; trim them and add the
		// non-null results to a list of lines; then return the list.
		public static List<string> ReadFileLines(string file)
		{
			FileStream f = new FileStream(file, FileMode.Open, FileAccess.Read);
			StreamReader reader = new StreamReader(f);
            return ReadStreamLines(reader);
        }
		
		protected void AddParsedReferences(AssetDefinition def, string referee, List<string> references,
										   Dictionary<string, List<string>> introducers, string introducer,
										   AssetTypeEnum assetType, AssetFileEnum fileType)
		{
			List<string> usedNames;
			if (introducers.TryGetValue(introducer, out usedNames)) {
				string dir = AssetFile.DirectoryForFileEnum(assetType, false, fileType);
				foreach (string referredTo in usedNames) {
                    string referredToFile = MakeFullFilePath(dir, referredTo);
					if (references.IndexOf(referredToFile) >= 0)
						continue;
					if (!new FileInfo(MakeRepositoryFilePath(referredToFile)).Exists) {
						Warn("Skipping file '{0}', referenced by '{1}', because the file does not exist",
							 referredToFile, referee);
						continue;
					}
					references.Add(referredToFile);
					AssetFile assetFile = new AssetFile(referredToFile, fileType);
					def.Files.Add(assetFile);
				}
			}
		}
		
		protected void AddReferencedShaders(AssetDefinition def, string referee, List<string> shaderFiles,
											Dictionary<string, List<string>> introducers, string introducer)
		{
			List<string> shaderNames;
			string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.Material, false, AssetFileEnum.Shader);
			if (introducers.TryGetValue(introducer, out shaderNames)) {
				foreach (string shaderName in shaderNames) {
					List<string> shaderPathnames;
					if (shaderNameToShaderFileNames.TryGetValue(shaderName, out shaderPathnames)) {
						foreach (string shaderFile in shaderPathnames) {
                            string file = MakeFullFilePath(dir, Path.GetFileName(shaderFile));
							ScriptParser.AddIfNotPresent(shaderFiles, file);
						}
					}
					else
						Warn("Shader '{0}', referenced by material script '{1}', does not exist in any shader file",
							 shaderName, referee);
				}
			}
		}
		
		// Provide an automated way to build material file assets
		protected void BuildAllMaterialAssets()
		{
			AssetTypeEnum assetType = AssetTypeEnum.Material;
			AssetFileEnum fileType = AssetFileEnum.Material;
			List<string> extensions = AssetFile.AllExtensionsForEnum(fileType);
			string matDir = AssetFile.DirectoryForFileEnum(assetType, true, fileType);
			DirectoryInfo info = new DirectoryInfo(MakeRepositoryFilePath(matDir));
 			FileInfo[] files = info.GetFiles();
 			foreach (FileInfo fileInfo in files) {
				string extension = fileInfo.Extension.ToLower();
				if (extensions.IndexOf(extension) < 0)
					continue;
				string baseName = Path.GetFileNameWithoutExtension(fileInfo.Name) + "_" +
					AssetTypeDesc.AssetTypeEnumFileName(assetType);
				string defPath = MakeRepositoryFilePath("AssetDefinitions\\" + baseName + ".asset");
				if (new FileInfo(defPath).Exists) {
					Warn("Skipping generated of asset definition '{0}', because it already exists", defPath);
					continue;
				}
				Dictionary<string, List<string>> uses = ScriptParser.ParseMaterialScript(fileInfo.FullName);
                AssetFile assetFile = new AssetFile(MakeFullFilePath(matDir, fileInfo.Name), fileType);
				AssetDefinition def = new AssetDefinition(baseName, assetType, assetFile);
				// Add the textures
				List<string> references = new List<string>();
				AddParsedReferences(def, fileInfo.FullName, references, uses, "texture",
									 assetType, AssetFileEnum.Texture);
				List<string> shaderFiles = new List<string>();
				AddReferencedShaders(def, fileInfo.FullName, shaderFiles, uses, "vertex_program_ref");
				AddReferencedShaders(def, fileInfo.FullName, shaderFiles, uses, "fragment_program_ref");
				foreach (string shaderFile in shaderFiles) {
					string f = Path.GetFileName(shaderFile);
					def.Files.Add(new AssetFile(shaderFile,	AssetFileEnum.Shader));
				}
				def.SetDescriptionFromName();
				def.WriteXmlFile(defPath);
			}
		}
		
		protected void BuildAllParticleScriptAssets()
		{
			AssetTypeEnum assetType = AssetTypeEnum.ParticleScript;
			AssetFileEnum fileType = AssetFileEnum.ParticleScript;
			List<string> extensions = AssetFile.AllExtensionsForEnum(fileType);
			string particleDir = AssetFile.DirectoryForFileEnum(assetType, true, fileType);
			DirectoryInfo info = new DirectoryInfo(MakeRepositoryFilePath(particleDir));
			FileInfo[] files = info.GetFiles();
 			foreach (FileInfo fileInfo in files) {
				string extension = fileInfo.Extension.ToLower();
				if (extensions.IndexOf(extension) < 0)
					continue;
				string baseName = Path.GetFileNameWithoutExtension(fileInfo.Name) + "_" + 
					AssetTypeDesc.AssetTypeEnumFileName(assetType);
				string defPath = MakeRepositoryFilePath("AssetDefinitions\\" + baseName + ".asset");
				if (new FileInfo(defPath).Exists) {
					Warn("Skipping generated of asset definition '{0}', because it already exists", defPath);
					continue;
				}
                AssetFile assetFile = new AssetFile(MakeFullFilePath(particleDir, fileInfo.Name), fileType);
				AssetDefinition def = new AssetDefinition(baseName, assetType, assetFile);
				// Add the textures
				Dictionary<string, List<string>> uses = ScriptParser.ParseParticleScript(fileInfo.FullName);
				List<string> references = new List<string>();
				AddParsedReferences(def, fileInfo.FullName, references, uses, "material",
									 assetType, AssetFileEnum.Material);
				def.SetDescriptionFromName();
				def.WriteXmlFile(defPath);
			}
		}
		
		protected void BuildSingleFileAssets(string dir, AssetTypeEnum assetType, AssetFileEnum fileType)
		{
			List<string> extensions = AssetFile.AllExtensionsForEnum(fileType);
			DirectoryInfo info = new DirectoryInfo(MakeRepositoryFilePath(dir));
			FileInfo[] files = info.GetFiles();
			foreach (FileInfo fileInfo in files) {
				string extension = fileInfo.Extension.ToLower();
				if (extensions.IndexOf(extension) < 0)
					continue;
				string baseName = Path.GetFileNameWithoutExtension(fileInfo.Name) + "_" + 
					AssetTypeDesc.AssetTypeEnumFileName(assetType);
				string defPath = MakeRepositoryFilePath("AssetDefinitions\\" + baseName + ".asset");
				if (new FileInfo(defPath).Exists) {
					Warn("Skipping generated of asset definition '{0}', because it already exists", defPath);
					continue;
				}
				AssetFile file = new AssetFile(MakeFullFilePath(dir, fileInfo.Name), fileType);
				AssetDefinition def = new AssetDefinition(baseName, assetType, file);
				def.SetDescriptionFromName();
				def.WriteXmlFile(defPath);
			}
		}
		
		protected void BuildAllSoundAssets()
		{
			string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.Sound, true, AssetFileEnum.Sound);
			BuildSingleFileAssets(dir, AssetTypeEnum.Sound, AssetFileEnum.Sound);
		}
		
		protected void BuildAllSpeedTreeAssets()
		{
			string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.SpeedTree, true, AssetFileEnum.SpeedTree);
			BuildSingleFileAssets(dir, AssetTypeEnum.SpeedTree, AssetFileEnum.SpeedTree);
		}

		protected void BuildAllSpeedWindAssets()
		{
			string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.SpeedWind, true, AssetFileEnum.SpeedWind);
			BuildSingleFileAssets(dir, AssetTypeEnum.SpeedWind, AssetFileEnum.SpeedWind);
		}

		public void BuildPlantTypeAssets()
		{
			AssetTypeEnum assetType = AssetTypeEnum.PlantType;
			XmlDocument doc = new XmlDocument();
			doc.Load(MakeRepositoryFilePath("Textures\\DetailVeg.imageset"));
			XmlElement root = doc.DocumentElement;
			XmlElement child = (XmlElement)root.FirstChild;
			while(child != null) {
				switch(child.LocalName) {
				case "ImageRect":
					string name = (string)child.Attributes[0].InnerText ;
					AssetFile file = new AssetFile("Textures\\DetailVeg.imageset", AssetFileEnum.ImageSet);
					AssetDefinition def = new AssetDefinition(name, AssetTypeEnum.PlantType, file);
					def.Files.Add(new AssetFile("AssetDefinitions\\DetailVeg_Material.asset", 
												AssetFileEnum.AssetDef));
					string baseName = name + "_" + AssetTypeDesc.AssetTypeEnumFileName(assetType);
                    def.SetDescriptionFromName();
					string defPath = "AssetDefinitions\\" + baseName + ".asset";
					def.WriteXmlFile(MakeRepositoryFilePath(defPath));
					break;
				default:
					break;
				}
				child = (XmlElement)child.NextSibling;
			}
		}
		
		public void BuildAllMeshAssets()
		{
			AssetTypeEnum assetType = AssetTypeEnum.Mesh;
			AssetFileEnum fileType = AssetFileEnum.Mesh;
			string meshDir = AssetFile.DirectoryForFileEnum(assetType, true, fileType);
			List<string> extensions = AssetFile.AllExtensionsForEnum(fileType);
			DirectoryInfo info = new DirectoryInfo(MakeRepositoryFilePath(meshDir));
			FileInfo[] files = info.GetFiles();
			foreach (FileInfo fileInfo in files) {
				string extension = fileInfo.Extension.ToLower();
				if (extensions.IndexOf(extension) < 0)
					continue;
				string fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
				string baseName = fileName + "_" + AssetTypeDesc.AssetTypeEnumFileName(assetType);
				string meshFile = MakeFullFilePath(meshDir, fileInfo.Name);
				AssetFile file = new AssetFile(meshFile, fileType);
				AssetDefinition def = new AssetDefinition(baseName, assetType, file);
				// Find the corresponding material and physics files
				string materialFile = MakeFullFilePath(AssetFile.DirectoryForFileEnum(assetType, false, AssetFileEnum.Material),
													   fileName + ".material");
				if (File.Exists(MakeRepositoryFilePath(materialFile))) {
					string assetDir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.Mesh, false,
																	 AssetFileEnum.AssetDef);
					string materialAsset = MakeFullFilePath(assetDir, fileName + "_Material.asset");
					file = new AssetFile(materialAsset, AssetFileEnum.AssetDef);
					def.Files.Add(file);
				}
				else
					Warn("For mesh '{0}', didn't find material file '{1}'",
						 meshFile, materialFile);
				string physicsFile = MakeFullFilePath(AssetFile.DirectoryForFileEnum(assetType, false, AssetFileEnum.Physics),
													  fileName + ".physics");
				if (File.Exists(MakeRepositoryFilePath(physicsFile))) {
					file = new AssetFile(physicsFile, AssetFileEnum.Physics);
					def.Files.Add(file);
				}
				else
					Warn("For mesh '{0}', didn't find physics file '{1}'",
						 meshFile, physicsFile);
				// It's a long shot, and misleading in the case of the
				// rocketboxers, but see if there is a matching skeleton
				string skeletonFile = MakeFullFilePath(AssetFile.DirectoryForFileEnum(assetType, false, AssetFileEnum.Skeleton),
													   fileName + ".skeleton");
				if (File.Exists(MakeRepositoryFilePath(skeletonFile))) {
					file = new AssetFile(skeletonFile, AssetFileEnum.Skeleton);
					def.Files.Add(file);
				}
				else
					Warn("For mesh '{0}', didn't find skeleton file '{1}'",
						 meshFile, skeletonFile);
				def.SetDescriptionFromName();
				string defPath = MakeRepositoryFilePath("AssetDefinitions\\" + baseName + ".asset");
				def.WriteXmlFile(defPath);
			}
		}

        public void BuildAllMovieAssets()
        {
            string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.Movie, true, AssetFileEnum.Movie);
            BuildSingleFileAssets(dir, AssetTypeEnum.Movie, AssetFileEnum.Movie);
        }

		public void BuildAllAssetDefinitions(string outputFile, string kind)
		{
			this.outputFile = outputFile;
			if (outputFile != "")
				File.Delete(outputFile);
			if (kind == "" || kind == "Material")
				BuildAllMaterialAssets();
			if (kind == "" || kind == "ParticleScript")
				BuildAllParticleScriptAssets();
			if (kind == "" || kind == "SpeedTree")
				BuildAllSpeedTreeAssets();
			if (kind == "" || kind == "SpeedWind")
				BuildAllSpeedWindAssets();
			if (kind == "" || kind == "PlantType")
				BuildPlantTypeAssets();
			if (kind == "" || kind == "Sound")
				BuildAllSoundAssets();
			if (kind == "" || kind == "Mesh")
				BuildAllMeshAssets();
            if (kind == "" || kind == "Movie")
                BuildAllMovieAssets();
		}
		
		public void InitializeRepositoryPath()
		{
            repositoryDirectoryList = GetRepositoryDirectoriesFromRegistry();
		}

        public void InitializeRepositoryPath(List<string> directories) {
            repositoryDirectoryList = directories;;
        }
		
        public List<string> InitializeRepository()
        {
            return InitializeRepository(GetRepositoryDirectoriesFromRegistry());
        }
		
		public void CopyAssetFiles(List<string> filesToCopy, List<string> fileDestinations)
		{
			for (int i=0; i<filesToCopy.Count; i++)
				File.Copy(filesToCopy[i], MakeRepositoryFilePath(fileDestinations[i]), true);
		}

        public static string[] GetCategoriesForType(AssetTypeEnum type)
        {
            if (type == AssetTypeEnum.Mesh)
                return new string[] { "Buildings", "Props", "Wall", "Characters", "Creatures" };
            else if (type == AssetTypeEnum.Material)
				return new string[] { "", "Skybox" };
			return null;
        }
			
		public void ComputeAssetsWithPrimaryFileName(List<string> log)
		{
			assetsWithPrimaryFileName = new Dictionary<string, List<AssetDefinition>>();
			foreach (AssetDefinition def in assetDefinitions.Values) {
				if (def.Files.Count > 0) {
					AssetFile file = def.Files[0];
					string s = file.TargetFile;
					List<AssetDefinition> defs;
					if (!assetsWithPrimaryFileName.TryGetValue(s, out defs)) {
						defs = new List<AssetDefinition>();
						assetsWithPrimaryFileName.Add(s, defs);
					}
					defs.Add(def);
				}
			}
		}
		
		protected void MaybeAddAssetDefName(List<string> defs, string defName, List<string> log)
		{
			AssetDefinition def;
            if (defName.EndsWith(".asset"))
                defName = defName.Remove(defName.Length - 6);
			if (!assetDefinitions.TryGetValue(defName, out def)) {
				log.Add(string.Format("In AssetRepository.MaybeAddAssetDefName, could not find asset definition named '{0}'",
									  defName));
				return;
			}
			ScriptParser.AddIfNotPresent(defs, defName);
		}
		
        protected List<AssetDefinition> FindAssetsWithPrimaryFileName(string file, AssetFileEnum fileType, List<string> log)
		{
			List<AssetDefinition> assets;
            string dir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.None, true, fileType);
            string fullFile = MakeFullFilePath(dir, file);
			if (assetsWithPrimaryFileName.TryGetValue(fullFile, out assets))
				return assets;
			else {
				log.Add(string.Format("Couldn't find assets based on file '{0}'", file));
				return new List<AssetDefinition>();
			}
		}
		
		// Produces a list of the files (relative to the root of the
		// media directory) for all assets in the manifest file,
		// together with all assets in the list of addons files.  The
		// list returned is sorted by name.
		protected List<string> ProduceMediaFileNames(List<string> manifestFileLines, List<string> addons,
                                                     Dictionary<string, string> fileRenames, List<string> log)
		{
			List<string> includedDefinitions = new List<string>();
			List<string> fileNames = new List<string>();
            foreach (string mline in manifestFileLines) {
                string line = mline.Trim();
                if (line == "")
                    continue;
                char[] colon = ":".ToCharArray();
                string [] halves = line.Split(colon);
                if (halves.Length != 2)
                {
                    log.Add(string.Format("In AssetRepository.ProduceMediaFileNames, the world editor asset line '{0}' is malformed",
                            line));
                    continue;
                }
                string s = halves[1];
                switch (halves[0]) {
                case "Mesh":
                    // Go find all asset definitions that reference
                    // this mesh file
                    foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(s, AssetFileEnum.Mesh, log)) {
                        if (def.TypeEnum == AssetTypeEnum.Mesh) {
                            MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                            if (def.Files.Count >= 2 && def.Files[1].FileTypeEnum == AssetFileEnum.AssetDef)
                                MaybeAddAssetDefName(includedDefinitions, 
                                    Path.GetFileName(def.Files[1].TargetFile),
                                    log);
                        }
                    }
                    break;
                case "SpeedTree":
                    string withExt = s + ".tre";
                    foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(withExt, AssetFileEnum.SpeedTree, log)) {
                        if (def.TypeEnum == AssetTypeEnum.SpeedTree)
                            MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                case "SpeedWind":
                    foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(s, AssetFileEnum.SpeedWind, log)) {
                        if (def.TypeEnum == AssetTypeEnum.SpeedWind)
                            MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                case "PlantType":
                    foreach (AssetDefinition def in FindAssetsWithPrimaryFileName("DetailVeg.imageset", AssetFileEnum.Texture, log)) {
                        MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                case "Sound":
                    foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(s, AssetFileEnum.Sound, log)) {
                        MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                case "Texture":
                    string textureFile = "Textures\\" + halves[1];
                    ScriptParser.AddIfNotPresent(fileNames, textureFile);
                    break;
                case "Mosaic":
                    string withMMF = s + ".mmf";
                    foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(withMMF, AssetFileEnum.Mosaic, log)) {
                        MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                case "Movie":
                    string movieFile = "Movies\\" + s;
                    foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(movieFile, AssetFileEnum.Movie, log)) {
                        MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                case "ParticleEffect":
                    string particleSystemFile;
                    if (!particleSystemToParticleSystemFileName.TryGetValue(s, out particleSystemFile))
                        log.Add(string.Format("Could not find the file corresponding to particle system '{0}'", s));
                    else {
                        particleSystemFile = Path.GetFileName(particleSystemFile);
                        foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(particleSystemFile, AssetFileEnum.ParticleScript, log))
                            MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                case "Material":
                    string materialFile;
                    if (!materialNameToMaterialFileName.TryGetValue(s, out materialFile))
                        log.Add(string.Format("Could not find the file corresponding to material '{0}'", s));
                    else {
                        materialFile = Path.GetFileName(materialFile);
                        foreach (AssetDefinition def in FindAssetsWithPrimaryFileName(materialFile, AssetFileEnum.Material, log))
                            MaybeAddAssetDefName(includedDefinitions, def.Name, log);
                    }
                    break;
                default:
                    log.Add(string.Format("In AssetRepository.ProduceMediaFileNames, unknown asset type '{0}'",
                            halves[0]));
                    break;
                }
            }
			foreach (string addon in addons) {
				List<string> addonLines = ReadFileLines(addon);
				foreach (string adefName in addonLines) {
					string defName = adefName.Trim();
					if (defName == "")
						continue;
					if (defName[0] == '#')
						continue;
					MaybeAddAssetDefName(includedDefinitions, defName, log);
				}
			}
			// Now we have the complete list of asset definitions.
			// Iterate through them and add the files
			foreach (string defName in includedDefinitions) {
				AddAssetDefinitionFiles(fileNames, fileRenames, defName, log);
			}
			// Sort the result and return it
			fileNames.Sort();
			return fileNames;
		}
		
		protected void AddAssetDefinitionFiles(List<string> fileNames, Dictionary<string, 
                                               string> fileRenames, string defName, List<string> log)
		{
            if (!assetDefinitions.ContainsKey(defName)) {
                log.Add(string.Format("Could not find asset definition '{0}' in the repository", defName));
                return;
            }
            AssetDefinition def = assetDefinitions[defName];
			string assetDir = AssetFile.DirectoryForFileEnum(AssetTypeEnum.None, false, AssetFileEnum.AssetDef);
            string assetFile = MakeFullFilePath(assetDir, defName + ".asset");
//             if (fileNames.IndexOf(assetFile) >= 0)
//                 return;
            ScriptParser.AddIfNotPresent(fileNames, assetFile);
			foreach (AssetFile file in def.Files) {
                ScriptParser.AddIfNotPresent(fileNames, file.TargetFile);
				if (file.NewFileName != "")
                    fileRenames.Add(file.TargetFile, file.NewFileName);
				if (Path.GetExtension(file.TargetFile) == ".asset") {
					AddAssetDefinitionFiles(fileNames, fileRenames, Path.GetFileNameWithoutExtension(file.TargetFile), log);
				}
			}
		}

		
		public void BuildMediaDirectoryStructure(string newMediaTree)
		{
			// Build the new media directory hierarchy, if it doesn't
			// already exist
			foreach (string dir in repositoryDirectories) {
				string dirPath = MakeFullFilePath(newMediaTree, dir);
				if (!new DirectoryInfo(dirPath).Exists)
					Directory.CreateDirectory(dirPath);
			}
		}
		
		protected string MakeCopyFilePath(string target) 
        {
            foreach (string path in repositoryDirectoryList) {
                string filename = Path.Combine(path, target);
                if (File.Exists(filename))
                    return filename;
            }
            return null;
        }
        
        // Copy the given list of files from the media tree pointed to
		// by the current repositoryPath to the directory pointed to
		// by the newMediaTree argument.  The file names are relative
		// to the root of the media tree(s).
		protected void CopyMediaTree(List<string> fileNames, Dictionary<string, string> fileRenames,
                                     List<string> addonFiles, string newMediaTree, bool copyAssetDefinitions, List<string> log)
		{
			BuildMediaDirectoryStructure(newMediaTree);
			// Copy all the files to the new tree
			foreach (string fileName in fileNames) {
                string ext = Path.GetExtension(fileName);
				if (!copyAssetDefinitions && (ext == ".asset" || ext == ".assetlist"))
                    continue;
                string path = MakeCopyFilePath(fileName);
                if (path == null)
                    log.Add(string.Format("Unable to find file: '{0}'", fileName));
                else {
                    string destination;
                    if (!fileRenames.TryGetValue(fileName, out destination))
                        destination = fileName;
                    File.Copy(path,
                              MakeFullFilePath(newMediaTree, destination),
                              true);
                }
			}
            foreach (string path in addonFiles) {
                if (!File.Exists(path))
                    log.Add(string.Format("File '{0}' does not exist", path));
                else
				    File.Copy(path,
                              MakeFullFilePath(newMediaTree, Path.GetFileName(path)),
                              true);
            }
            log.Add(string.Format("Done: {0} asset files copied", fileNames.Count));
		}
		
		public void GenerateAndCopyMediaTree(List<string> manifestFileLines, List<string> addonFiles, 
                                             string newMediaTree, bool copyAssetDefinitions)
		{
            errorLog = new List<string>();
            try {
                errorLog.Add("Building the media directory structure");
                BuildMediaDirectoryStructure(newMediaTree);
                Dictionary<string, string> fileRenames = new Dictionary<string, string>();
                errorLog.Add("Determining the set of media file names");
                List<string> files = ProduceMediaFileNames(manifestFileLines, addonFiles, fileRenames, errorLog);
                errorLog.Add("Copying media files");
                CopyMediaTree(files, fileRenames, addonFiles, newMediaTree, copyAssetDefinitions, errorLog);
            }
            catch (Exception e) { 
                errorLog.Add("In GenerateAndCopyMediaTree, exception raised " + e);
            }
		}
		
        protected List<string> repositoryDirectoryList;
		protected string outputFile = "";
        protected List<string> errorLog;
		Dictionary<AssetFileEnum, Dictionary<string, AssetFileInfo>> assetFiles;
		Dictionary<string, AssetDefinition> assetDefinitions;
		Dictionary<string, List<AssetDefinition>> assetsWithPrimaryFileName;
		Dictionary<string, string> materialNameToMaterialFileName;
		Dictionary<string, List<string>> shaderNameToShaderFileNames;
		Dictionary<string, List<string>> materialFileNameToTextureNames;
        Dictionary<string, List<string>> materialFileNameToMovieNames;
        Dictionary<string, List<string>> materialFileNameToMaterialNames;
		Dictionary<string, string> particleSystemToParticleSystemFileName;
		Dictionary<string, List<string>> particleScriptFileNameToParticleSystemNames;
		Dictionary<string, List<string>> particleScriptFileNameToMaterialNames;
		Dictionary<string, List<string>> particleScriptFileNameToTextureNames;
		
        public void Warn(string format, params Object[] list)
        {
            string s = string.Format(format, list);
			if (outputFile != "") {
                FileStream f = new FileStream(outputFile,
											  (File.Exists(outputFile) ? FileMode.Append : FileMode.Create), 
											  FileAccess.Write);
                StreamWriter writer = new StreamWriter(f);
				writer.WriteLine(s);
				writer.Close();
			}
			else
				Console.WriteLine(s);
		}
			
		private static readonly RepositoryClass instance = new RepositoryClass();

		public static RepositoryClass Instance 
		{
			get { return instance; }
        }

        public List<string> RepositoryDirectoryList {
            get {
                return repositoryDirectoryList;
            }
            set {
                repositoryDirectoryList = value;
            }
        }

        public string RepositoryDirectoryListString {
            get {
                return MakeRepositoryDirectoryListString(repositoryDirectoryList);
            }
        }
        
        public string MakeRepositoryDirectoryListString(List<string> directories) {
            string s = "";
            if (directories == null || directories.Count == 0)
                return s;
            foreach (string dir in directories) {
                if (s != "")
                    s += "; ";
                s += dir;
            }
            return s;
        }

        public string LastRepositoryPath
        {
            get
            {
                if (repositoryDirectoryList != null && repositoryDirectoryList.Count > 0)
                    return repositoryDirectoryList[repositoryDirectoryList.Count - 1];
                else
                    return "";
            }
        }

        public string FirstRepositoryPath
        {
            get
            {
                if (repositoryDirectoryList != null && repositoryDirectoryList.Count > 0)
                    return repositoryDirectoryList[0];
                else
                    return "";
            }
        }        

		public string AssetDefinitionDirectory
		{
            get {
                string s = LastRepositoryPath;
                if (s != "")
                    return s + "\\AssetDefinitions";
                else
                    return "";
              }
		}
		
		public Dictionary<string, AssetDefinition>.ValueCollection AssetDefinitions
		{
            get { return assetDefinitions.Values; }
		}
		
		public static string[] AxiomDirectories
		{
			get { return axiomDirectories; }
		}
	
		public static string[] ClientResources
		{
			get { return clientResources; }
        }

        public List<string> ErrorLog {
            get { return errorLog; }
            set { errorLog = value; }
        }


    }
}
