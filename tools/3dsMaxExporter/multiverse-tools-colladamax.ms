-- Multiverse MAX tools
--
--   John Wainwright for Multiverse.net

-- Export Tool UI
--
macroScript MultiverseExportMacro
	category:"Multiverse Tools" 
	tooltip:"Opens the Multiverse Exporter Dialog"
	buttonText:"Export Tool"
(
	-- constants
	TOOL_VERSION = 1.34  -- 8/8/07
	MVE_CONFIG_FILE = "multiverse-tools.ini"
	
	LV_FIRST = 0x1000  -- ListView ActiveX control message IDs...
	LV_SETCOLUMNWIDTH = (LV_FIRST + 30)
	
	FILE_VERSION = 1.0
	
	-- structs
	-- model set element
	struct MSE 
	(
		modelDefFile = "",
		checked = true,
		LODStatus = "No LOD",
		LODBaseMesh = "",   
		LODDistance = 0
	)
	
	-- model
	struct Model
	(
		baseName = "",
		baseScene = "",
		modelType = "Static",
		encoding = "Single Scene File",
		staticBaseObjectName = "",
		bipObjectName = "",
		components = #(),
		assetCategory = "Character",
		assetDescription = "",
		repositories = #(),
		textures = #(),
		cvRadius = 20,
		cvHeight = 120,
		cvPos = [0,0,0],
		cvBaseMeshName = ""
	)
	
	-- animation component list entry
	struct Component
	(
		animName = "",
		checked = true,
		fileName = "Choose...",
		baseObjectName = "",
		startFrame = 0,
		endFrame = 0
	)
	
	-- repository entry
	struct Repository 
	(
		checked = true,
		rootDirectory = "Choose..."
	)
	
	-- globals
	global MVE_toolWindow   -- main tool dialog window
	
	-- macro locals: tool state
	local modelFile, modelChanged = false, selectedComponent, selectedRepository, modelSetFile, modelSetChanged = false
	local modelConverter, modelViewer, worldEditor, assetImporter, converterPath, converterExe, selectedModel
	local viewerPath, viewerExe, toolTimeout, logFileName
	local configFile = (getDir #plugcfg) + "\\" + MVE_CONFIG_FILE
	local m = Model()  -- current model cache
	local mro  		   -- current model detail rollout
	local modelSet = #() -- current model set, list of model set elements (MSEs)

	-- forwards
	local MVE_Repositories, MVE_Export, MVE_Model, MVE_Static, MVE_Animated_SF, MVE_Animated_BF, MVE_Animated_SS, 
		  MVE_Animated_FR, MVE_Config, loadUI, saveModel

	-- utility functions
	fn fitButtonText str width = 
	(
		-- fit button text to given pixel width by replacing middle text with "..."
		local w = 3
		while (getTextExtent str).x > width - 4 do (
			str = replace str ((str.count / 2) - w / 2) w "..."
			w += 1
		)
		return str
	)
	
	-- ensure all dirs in pathname exist
	fn makeDeepDir pathname = 
	(
		p = ""
		for d in filterString pathname "\\" do (
			p += d + "\\"
			if not doesFileExist p then makeDir p
		)
	)
	
	/* -------------- rollout utilitiy functions ------------- */
	
	-- (re)open rollouts depending on model type & encoding choice
	fn openRollouts =
	(
		local repRolled = not MVE_Repositories.open, 
			  expRolled = not MVE_Export.open, 
			  confRolled = not MVE_Config.open
			  
		if mro != undefined then removeRollout mro MVE_toolWindow
		removeRollout MVE_Repositories MVE_toolWindow
		removeRollout MVE_Export MVE_toolWindow 
		removeRollout MVE_Config MVE_toolWindow
		
		mro = undefined
		if m.modelType == "Static" and m.encoding == "Shared Scene File" then
			addRollout (mro = MVE_Static) MVE_toolWindow
		else case m.encoding of (
			"Scene Files": addRollout (mro = MVE_Animated_SF) MVE_toolWindow
			"BIP Files": addRollout (mro = MVE_Animated_BF) MVE_toolWindow
			"Shared Scene": addRollout (mro = MVE_Animated_SS) MVE_toolWindow
			"Frame Range": addRollout (mro = MVE_Animated_FR) MVE_toolWindow
		)
		
		addRollout MVE_Repositories MVE_toolWindow rolledUp:repRolled 
		addRollout MVE_Export MVE_toolWindow rolledUp:expRolled 
		addRollout MVE_Config MVE_toolWindow rolledUp:confRolled 
	)
	
	-- check if model changed & offer to save; return #cancel if cancelled
	fn checkIfNeedsSave = 
	(
		if modelChanged then (
			local ync = yesNoCancelBox "Current model not saved, save it now?"
			if ync == #yes then (
				if modelFile == undefined then (
					modelFile = getSaveFileName types:"Model(*.mdl)|*.mdl"
					if modelFile == undefined do return #cancel
				)
				saveModel()
			) 
			else if ync == #cancel do return #cancel
		)
		return #ok
	)
	
	-- save current model
	fn saveModel = 
	(
		-- header
		delINISetting modelFile "Header"
		setINISetting modelFile "Header" "signature" "Multiverse~MDL"
		setINISetting modelFile "Header" "version" (FILE_VERSION as string)
		
		-- model basics
		delINISetting modelFile "Model"
		setINISetting modelFile "Model" "baseName" m.baseName
		setINISetting modelFile "Model" "baseScene" m.baseScene
		setINISetting modelFile "Model" "modelType" m.modelType
		setINISetting modelFile "Model" "encoding" m.encoding
		delINISetting modelFile "Static"
		setINISetting modelFile "Static" "baseObjectName" m.staticBaseObjectName
		if m.encoding == "BIP Files" then (
			delINISetting modelFile "BIP"
			setINISetting modelFile "BIP" "bipObjectName" m.bipObjectName
		)
		-- anim components
		delINISetting modelFile "Components"
		setINISetting modelFile "Components" "count" (m.components.count as string)
		for i = 1 to m.components.count do (
			c = m.components[i]
			setINISetting modelFile "Components" ("animName_" + i as string) c.animName
			setINISetting modelFile "Components" ("checked_" + i as string) (if c.checked then "true" else "false")
			case m.encoding of (
				"Scene Files": setINISetting modelFile "Components" ("fileName_" + i as string) c.fileName
				"BIP Files": setINISetting modelFile "Components" ("fileName_" + i as string) c.fileName
				"Shared Scene": setINISetting modelFile "Components" ("baseObjectName_" + i as string) c.baseObjectName
				"Frame Range": (
					setINISetting modelFile "Components" ("startFrame_" + i as string) (c.startFrame as string)
					setINISetting modelFile "Components" ("endFrame_" + i as string) (c.endFrame as string)
				)
			)
		)
		setINISetting modelFile "CollisionVolume" "capsuleRadius" (m.cvRadius as string)
		setINISetting modelFile "CollisionVolume" "capsuleHeight" (m.cvHeight as string)
		setINISetting modelFile "CollisionVolume" "capsulePos" (m.cvPos as string)
		setINISetting modelFile "CollisionVolume" "cvBaseMeshName" m.cvBaseMeshName
		-- repositories
		delINISetting modelFile "Repositories"
		setINISetting modelFile "Repositories" "assetCategory" m.assetCategory
		setINISetting modelFile "Repositories" "assetDescription" m.assetDescription
		setINISetting modelFile "Repositories" "count" (m.repositories.count as string)
		for i = 1 to m.repositories.count do (
			setINISetting modelFile "Repositories" ("rootDirectory_" + i as string) m.repositories[i].rootDirectory
			setINISetting modelFile "Repositories" ("checked_" + i as string) (m.repositories[i].checked as string)
		)
		
		setINISetting configFile "Recent" "ModelDef" modelFile 
		modelChanged = false
	)
	
	-- load new model def file
	fn loadModel f loadIntoUI:true = 
	(
		local version, numComponents, numRepositories
			  
		-- check signature & version
		if getINISetting f "Header" "signature" != "Multiverse~MDL" then (
			messageBox (f + " is not a valid Multiverse model definition file!") beep:true
			return false
		)
		version = (getINISetting f "Header" "version") as Number
		if version <= 0 or version > FILE_VERSION then (	
			messageBox (f + ": Incompatible version of Multiverse model definition file.") beep:true
			return false
		)
		
		-- load into locals
		m.baseName = getINISetting f "Model" "baseName"
		m.baseScene = getINISetting f "Model" "baseScene"
		m.modelType = getINISetting f "Model" "modelType"
		m.encoding = getINISetting f "Model" "encoding"
		m.staticBaseObjectName = getINISetting f "Static" "baseObjectName"
		if m.encoding == "BIP Files" then (
			m.bipObjectName = getINISetting f "BIP" "bipObjectName" 
		)
		-- animation components
		m.components = #()
		if m.modelType == "Animated" then (
			numComponents = (getINISetting f "Components" "count") as number
			for i in 1 to numComponents do (
				local animName = getINISetting f "Components" ("animName_" + i as string),
				      checked = getINISetting f "Components" ("checked_" + i as string),
					  fileName = getINISetting f "Components" ("fileName_" + i as string),
					  baseObjectName = getINISetting f "Components" ("baseObjectName_" + i as string),
					  startFrame = getINISetting f "Components" ("startFrame_" + i as string),
					  endFrame = getINISetting f "Components" ("endFrame_" + i as string)
				append m.components (Component animName (checked == "true") fileName baseObjectName (startFrame as integer) (endFrame as integer))
			)
		)
		m.cvRadius = try (getINISetting f "CollisionVolume" "capsuleRadius") as float catch 20
		m.cvHeight = try (getINISetting f "CollisionVolume" "capsuleHeight") as float catch 120
		m.cvPos = try readValue ((getINISetting f "CollisionVolume" "capsulePos" as stringStream)) catch [0,0,0]
		m.cvBaseMeshName = getINISetting f "CollisionVolume" "cvBaseMeshName"
		m.assetCategory = getINISetting f "Repositories" "assetCategory"
		m.assetDescription = getINISetting f "Repositories" "assetDescription"
		-- repositories
		m.repositories = #()
		numRepositories = (getINISetting f "Repositories" "count") as number
		for i in 1 to numRepositories do (
			local rootDir = getINISetting f "Repositories" ("rootDirectory_" + i as string),
			      checked = getINISetting f "Repositories" ("checked_" + i as string)
			append m.repositories (Repository checked:(checked == "true") rootDirectory:rootDir)
		)
		
		-- load into UI
		if loadIntoUI then loadUI()
		
		modelChanged = false

		-- success
		return true
	)
	
	-- fill the model encoding dropdown basedon model type
	fn setModelEncodings =
	(
		MVE_Model.encoding.items = 
			if MVE_Model.modelType.selection == 1 then #("Single Scene File", "Shared Scene File") -- static
											      else #("Scene Files", "BIP Files", "Shared Scene" ) --, "Frame Range") -- animated
		MVE_Model.encoding.selection = 1
	)
	
	--  update all UI dropdowns & control enable states
	fn setCVCapsuleUIState =
	(
		if mro != undefined then (
			mro.editCapsule.enabled = mro.capsuleHeight.enabled = mro.capsuleRadius.enabled = m.cvBaseMeshName != ""
		)
		if m.cvBaseMeshName == "" then 
			MVE_Export.enableAddPhysics.enabled = MVE_Export.enableAddPhysics.checked = false
		else (
			if not MVE_Export.enableAddPhysics.enabled do MVE_Export.enableAddPhysics.checked = true
			MVE_Export.enableAddPhysics.enabled = true
		)
	)	
	
	fn setUIStates = 
	(
		if m.modelType == "Animated" then (
			mro.delComponent.enabled = mro.animName.enabled = selectedComponent != undefined
			case m.encoding of (
				"Scene Files":  mro.L6.enabled = mro.loadAnim.enabled = mro.pickComponentFile.enabled = selectedComponent != undefined
				"BIP Files":    mro.L6.enabled = mro.loadAnim.enabled = mro.saveAnim.enabled = mro.pickComponentFile.enabled = selectedComponent != undefined
				"Shared Scene": mro.animBaseObjectName.enabled = mro.loadAnim.enabled = selectedComponent != undefined
				"Frame Range":  mro.fromFrame.enabled = mro.toFrame.enabled = mro.loadAnim.enabled = selectedComponent != undefined
			)
			setCVCapsuleUIState()
		)
		else if m.modelType == "Static" then (
		)
		MVE_Export.setUIStates()
	)

	-- load anim components into UI
	fn loadComponents = if m.modelType == "Animated" then
	(
		mro.components.listItems.clear()
		
		if m.modelType == "Animated" then (
			-- column headers
			cols = #(#(#("     Animation", 100), #("Scene File", 170), #("Scene File Path", 800)),
					 #(#("     Animation", 100), #("BIP File", 170), #("BIP File Path", 800)),
					 #(#("     Animation", 100), #("Base Object Name", 295)),
					 #(#("     Animation", 100), #("Start Frame", 94), #("End Frame", 94)))[MVE_Model.encoding.selection]
			if mro.components.columnHeaders.count == 0 or mro.components.columnHeaders[2].text != cols[2][1] then (
				-- new column layout, reset
				mro.components.columnHeaders.clear()
				for i = 1 to cols.count do (
					mro.components.columnHeaders.add text:cols[i][1]
					-- hack to set column width
					windows.sendMessage mro.components.hwnd LV_SETCOLUMNWIDTH (i-1) cols[i][2]
				)
			)
			-- add components
			for c in m.components do (
				local li = mro.components.listItems.add text:(" " + c.animName)
				li.checked = c.checked
				local sli = li.listSubItems.add()
				case m.encoding of (
					"Scene Files": (
						sli.text = getFileNameFile c.fileName
						(li.listSubItems.add()).text = c.fileName
						if stricmp c.animName "base" == 0 then
							messageBox "Warning: Base animation is taken from base scene file when using Scene File encoding.\nAny animation component named \"base\" will not be included in export."
					)
					"BIP Files": (
						sli.text = getFileNameFile c.fileName
						(li.listSubItems.add()).text = c.fileName
					)
					"Shared Scene": sli.text = c.baseObjectName
					"Frame Range": (
						sli.text = c.startFrame as string
						sli = li.listSubItems.add()
						sli.text = c.endFrame as string
					)
				)
				if c == selectedComponent do li.selected = true
			)
			mro.components.enabled = true
			mro.components.dropHighlight = mro.components.selectedItem
			if mro.components.selectedItem != undefined then mro.components.selectedItem .ensureVisible()
			mro.L4.text = "Animation Components [" + m.components.count as string + "]"
		)
		
	)	
	
	-- load component selected in components listview into UI fields
	fn loadSelectedComponent = if m.modelType == "Animated" then
	(
		mro.animName.text = ""
		case m.encoding of (
			"Scene Files": (mro.pickComponentFile.text = "Choose..."; mro.pickComponentFile.toolTip = "")
			"BIP Files":   (mro.pickComponentFile.text = "Choose..."; mro.pickComponentFile.toolTip = "")
			"Shared Scene": mro.animBaseObjectName.text = ""
			"Frame Range":  mro.fromFrame.value = mro.toFrame.value = 0
		)
		if selectedComponent != undefined then (
			local c = selectedComponent,
			      fname = if c.fileName == "" then "Choose..." else fitButtonText c.fileName 250,
				  tt = if c.fileName == "" then "Click to pick a file." else c.fileName
			mro.animName.text = c.animName
			
			case m.encoding of (
				"Scene Files": (mro.pickComponentFile.text = fname; mro.pickComponentFile.toolTip = tt)
				"BIP Files": (mro.pickComponentFile.text = fname; mro.pickComponentFile.toolTip = tt)
				"Shared Scene": mro.animBaseObjectName.text = c.baseObjectName
				"Frame Range": (mro.fromFrame.value = c.startFrame; mro.toFrame.value = c.endFrame)
			)
		)
	)
	
	-- handle component selection
	fn componentSelected = 
	(
		local li = mro.components.selectedItem
		selectedComponent = if li != undefined then m.components[li.index] else undefined
		mro.components.dropHighlight = li
		setUIStates()
		loadSelectedComponent()
	)
		
	-- load UI from rollout locals
	fn loadUI = 
	(
		-- open appropriate rollouts
		openRollouts()

		--  update all UI dropdowns & control enable states
		MVE_Model.modelType.selection = findItem MVE_Model.modelType.items m.modelType
		setModelEncodings()
		MVE_Model.encoding.selection = findItem MVE_Model.encoding.items m.encoding
		setUIStates()
		
		-- model details
		MVE_Model.baseName.text = m.baseName
		if m.baseScene == "" then (
			MVE_Model.pickBaseScene.text = "Choose..."; MVE_Model.pickBaseScene.toolTip = "Choose base scene file"
		)
		else (
			MVE_Model.pickBaseScene.text = fitButtonText m.baseScene 250; MVE_Model.pickBaseScene.toolTip = m.baseScene
		)
		MVE_Static.staticBaseObjectName.text = m.staticBaseObjectName
		if m.encoding == "BIP Files" then (
			if m.bipObjectName== "" then (
				MVE_Animated_BF.pickBIPObject.text = "Pick..."
			)
			else (
				MVE_Animated_BF.pickBIPObject.text = fitButtonText m.bipObjectName 250
			)
		)
		loadComponents()
		loadSelectedComponent()
		if m.modelType == "Animated" then (
			mro.capsuleHeight.value = m.cvHeight
			mro.capsuleRadius.value = m.cvRadius
			if m.cvBaseMeshName == "" then
				mro.pickCVSubMesh.text = "Pick..."
			else
				mro.pickCVSubMesh.text = fitButtonText m.cvBaseMeshName 160 
		)

		local catIndex = findItem MVE_Repositories.assetCategoryDD.items m.assetCategory
		MVE_Repositories.assetCategoryDD.selection = if catIndex > 0 then catIndex else 1
		MVE_Repositories.assetCategory.text = m.assetCategory
		MVE_Repositories.description.text = m.assetDescription
		MVE_Repositories.loadRepositories()
		MVE_Repositories.repositorySelected()
	)
	
	-- clear model def data in rollout 
	fn clearModel =
	(
		m.baseName = ""
		m.baseScene = ""
		m.modelType = "Static"
		m.encoding = "Single Scene File"
		m.staticBaseObjectName = ""
		m.components = #()
		m.assetCategory = "Character"
		m.assetDescription = ""
		m.repositories = #()
		m.bipObjectName = ""
		m.cvRadius = 20
		m.cvHeight = 120
		m.cvPos = [0,0,0]
		m.cvBaseMeshName = ""
		
		-- clear selections
		selectedComponent = selectedRepository = undefined
		
		-- load UI
		loadUI()
		
		modelChanged = false
	)
			
	fn componentRolloutOpen ro = 
	(
		-- configure components listview
		ro.components.gridLines = true  
		ro.components.View = #lvwReport  
		ro.components.fullRowSelect = true 
		ro.components.checkboxes = true
		ro.components.labelEdit = #lvwManual
	)


	/* ---- animation component handler common fns  ------- */
	
	--  add new component
	fn addComponent_pressed = 
	(
		append m.components (Component())
		loadComponents()
		mro.components.listItems[mro.components.listItems.count].selected = true
		componentSelected()
		if mro == MVE_Animated_SF or mro == MVE_Animated_BF then (
			mro.pickComponentFile.pressed()
			if selectedComponent != undefined and selectedComponent.fileName != "Choose..." then (
				selectedComponent.animName = getFileNameFile selectedComponent.fileName 
				mro.animName.text = selectedComponent.animName 
				loadComponents()
			)
		)
		mro.components.dropHighlight = mro.components.selectedItem
		mro.components.selectedItem.ensureVisible()
		setFocus mro.animName
 		modelChanged = true
	)
	
	-- delete selected component
	fn delComponent_pressed = 
	(
		local li = mro.components.selectedItem
		if li != undefined then (
			local i = li.index
			deleteItem m.components i
			loadComponents()
			-- select next component in list or end of list if last
			if i > m.components.count then i = m.components.count
			if i > 0 then mro.components.listItems[i].selected = true
			componentSelected()
			modelChanged = true
		)
	)
	
	-- handle click on component listview entry
	fn components_itemClick ro: =
	(
		componentSelected()
		-- short-cut to file name picker if name is "Choose..." when selected
		if ro != unsupplied and selectedComponent != undefined and selectedComponent.fileName == "Choose..." then
			ro.pickComponentFile.pressed()
	)
	
	-- handle check change on component listview entry
	fn components_itemCheck item =
	(
		m.components[item.index].checked = item.checked
		modelChanged = true
	)
	
	-- handle new animation name
	fn animName_entered newVal =
	(
		if selectedComponent != undefined then (
			selectedComponent.animName = newVal
			loadComponents()
			MVE_Export.setUIStates()
			modelChanged = true
		)
	)
	
	-- animated model colision volume capsule
	fn editCapsule_changed state ro = 
	(
		if state then ( -- on
			if $__tmn_collision_capsule != undefined then delete $__tmn_collision_capsule
			cv = capsule radius:m.cvRadius height:m.cvHeight sides:6 heightSegs:1 smooth:true \
			     sliceOn:false heightType:0 name:"__tmn_collision_capsule" pos:m.cvPos
			cv.material = standardMaterial diffuseColor:(color 151 172 240) opacity:50
			ro.capsuleRadius.enabled = ro.capsuleHeight.enabled = true
			nodeInvalRect cv
			redrawViews()
		)
		else (  -- off
			if $__tmn_collision_capsule != undefined then (
				m.cvPos = $__tmn_collision_capsule.pos
				delete $__tmn_collision_capsule	
			)
			ro.capsuleRadius.enabled = ro.capsuleHeight.enabled = false
		)
	)
	
	fn capsuleRadius_changed val ro = 
	(
		m.cvRadius = val
		cv = $__tmn_collision_capsule
		if cv != undefined then (
			cv.radius = val
			m.cvRadius = cv.radius
			m.cvHeight = cv.height -- in case lower-bound enforced by capsule
			ro.capsuleHeight.range = [2*cv.radius, 1000, cv.height]
			m.cvPos = cv.pos
		)
		modelChanged = true
	)
	
	fn capsuleHeight_changed val = 
	(
		m.cvHeight = val
		cv = $__tmn_collision_capsule
		if cv != undefined then (
			cv.height = val
			m.cvHeight = cv.height -- in case lower-bound enforced by capsule
			m.cvPos = cv.pos
		)
		modelChanged = true
	)
	
	fn pickCVSubMesh_picked obj ro = 
	(
		m.cvBaseMeshName = obj.name
		ro.pickCVSubMesh.text = fitButtonText obj.name 160
		m.cvPos = [obj.center.x, obj.center.y, obj.min.z] 
		if $__tmn_collision_capsule != undefined then $__tmn_collision_capsule.pos = m.cvPos
		modelChanged = true
		setUIStates()
		MVE_Export.setUIStates()
	)

	/* ---------------- tool UI rollout defs -------------------------- */
	
	-- allows a model set to be defined
	rollout MVE_ModelSet "Model Set"
	(
		local saveModelSet, loadModelSet, rearrangedModelSet 
		
		label L0 "Model Set:" align:#left
		button pickModelSet "Load..." width:250
		--button newModelSetBtn "New Set" across:2 offset:[90,0]
		--button saveModelSetBtn "Save Set" offset:[30,0]
		button newModelSetBtn "Reset" across:3 offset:[63,0] width:55
		button saveModelSetBtn "Save" offset:[37,0] width:55
		button saveAsModelSetBtn "Save As" offset:[12,0] width:55
		label L1 "Model Definitions:" align:#left
		activeXControl models "MSComctlLib.ListViewCtrl" width:275 height:200 align:#center
		button addModel "Add Model Def." across:3
		button delModel "Delete Model Def." offset:[16,0]
		button loadModelBtn "Load" offset:[28,0]
		
		group "Model Def Details"
		(
			label l1a "Selected Model Def:   --" align:#left offset:[14,0]
			label L2 "Manual LOD Status:" across:2 align:#right offset:[-17,4]
			dropDownList lodStatus offset:[-10,0] items:#("No LOD", "Base Mesh", "LOD Level")
			label L3 "LOD Base Mesh Model:" across:2 align:#right offset:[-17,4] enabled:false
			dropDownList lodBaseMesh offset:[-10,0] items:#("Foo", "Baz", "Bar") enabled:false
			spinner lodDistance "LOD Distance (meters): " type:#integer range:[0, 10000, 0] offset:[-10,0] enabled:false
		)
		
		fn setUIStates = 
		(
			lodStatus.enabled = models.selectedItem != undefined
			L3.enabled = lodBaseMesh.enabled = lodDistance.enabled = lodStatus.enabled and lodStatus.selected == "LOD Level"
			if lodStatus.enabled and lodStatus.selected == "LOD Level" then
				lodBaseMesh.items = for m in modelSet where m.lodStatus == "Base Mesh" collect getINISetting m.modelDefFile "Model" "baseName"
			else
				lodBaseMesh.items = #()
		)
		
		-- load detail UI from selected model def
		fn loadModelUI = 
		(
			if selectedModel != undefined then (
				l1a.text = "Selected Model Def:   " + (getINISetting selectedModel.modelDefFile "Model" "baseName")
				lodStatus.selection = findItem lodStatus.items selectedModel.lodStatus
				local i = findItem lodBaseMesh.items selectedModel.lodBaseMesh
				if i > 0 then lodBaseMesh.selection = i
				lodDistance.value = if selectedModel.lodStatus == "LOD Level" then selectedModel.lodDistance else 0
			)
			else (
				l1a.text = "Selected Model Def:   --"
				lodStatus.selection = 1
				lodBaseMesh.selection = 0
				lodDistance.value = 0
			)
		)
		
		-- handle model def selection
		fn modelSelected = 
		(
			models.dropHighlight = undefined
			local li = models.selectedItem
			if li != undefined then (
				selectedModel = modelSet[li.index]
				loadModelUI()
				models.dropHighlight = li
			)
			else (
				selectedModel = undefined
				loadModelUI()
			)
			setUIStates()
		)
				
		fn loadSetUI = 
		(
			rearrangedModelSet = #()
			models.listItems.clear()
			fn loadModelIntoList e foreColor:black indent:"" bold:false = 
			(
				local name = getINISetting e.modelDefFile "Model" "baseName",
					  li = models.listItems.add text:(" " + indent + (if name == "" then "!! missing !!" else name)),
					  lsi = li.listSubItems.add()
				lsi.text = e.modelDefFile
				li.foreColor = lsi.foreColor = foreColor
				li.bold = lsi.bold = bold 
				li.checked = e.checked and name != ""
				if e == selectedModel then li.selected = true
				append rearrangedModelSet e
				return name
			)
			-- collect base LOD models & add lod sets first
			local bases = for e in modelSet where e.lodStatus == "Base Mesh" collect e,
			      baseNames = #()
			for b in bases do (
				-- pick up model names from def file
				local bname = loadModelIntoList b bold:true -- foreColor:(color 255 10 10)
				append baseNames bname
				-- add level models for this lod-group
				local lodSet = for e in modelSet where e.lodStatus == "LOD Level" and e.lodBaseMesh == bname collect e
				qsort lodSet (fn lscf e1 e2 = if e1.lodDistance < e2.lodDistance then -1 else if e1.lodDistance == e2.lodDistance then 0 else 1)
				for e in lodSet do loadModelIntoList e indent:"   " -- foreColor:(color 10 255 10)
			)
			-- reset models to No LOD if their LOD base was deleted
			for e in modelSet where e.lodStatus == "LOD Level" and findItem baseNames e.lodBaseMesh == 0 do
				e.lodStatus = "No LOD"
			-- finally non-LOD models
			for e in modelSet where e.lodStatus == "No LOD" do loadModelIntoList e

			-- replace modelSet with one arranged in listview order so selection click indexes work
			modelSet = rearrangedModelSet 
			modelSelected()
			if models.selectedItem != undefined then models.selectedItem.ensureVisible()
		)	
		
		-- clear model set def data in rollout 
		fn clearModelSet =
		(
			modelSet = #()
			loadSetUI()
			modelSetChanged = false
			selectedModel = undefined
		)
				
		fn checkIfSetNeedsSave = 
		(
			if modelSetChanged then (
				local ync = yesNoCancelBox "Current model set not saved, save it now?"
				if ync == #yes then (
					if modelSetFile == undefined then (
						modelSetFile = getSaveFileName types:"Model Set(*.mds)|*.mds"
						if modelSetFile == undefined do return #cancel
					)
					saveModelSet()
				) 
				else if ync == #cancel do return #cancel
			)
			return #ok
		)
		
		-- save current model set
		fn saveModelSet = 
		(
			-- header
			delINISetting modelSetFile "Header"
			setINISetting modelSetFile "Header" "signature" "Multiverse~MDS"
			setINISetting modelSetFile "Header" "version" (FILE_VERSION as string)
			
			-- set elements
			delINISetting modelSetFile "ModelSet"
			setINISetting modelSetFile "ModelSet" "count" (modelSet.count as string)
			for i = 1 to modelSet.count do (
				cm = modelSet[i]
				setINISetting modelSetFile "ModelSet" ("modelDefFile_" + i as string) cm.modelDefFile
				setINISetting modelSetFile "ModelSet" ("checked_" + i as string) (cm.checked as string)
				setINISetting modelSetFile "ModelSet" ("LODStatus_" + i as string) cm.LODStatus 
				setINISetting modelSetFile "ModelSet" ("LODBaseMesh_" + i as string) cm.LODBaseMesh
				setINISetting modelSetFile "ModelSet" ("LODDistance_" + i as string) (cm.LODDistance as string)
			)
			setINISetting configFile "Recent" "ModelSet" modelSetFile 
			modelSetChanged = false
		)
		
		-- load new model def file
		fn loadModelSet f = 
		(
			local version, numModels
				  
			-- check signature & version
			if getINISetting f "Header" "signature" != "Multiverse~MDS" then (
				messageBox (f + " is not a valid Multiverse model set definition file!") beep:true
				return false
			)
			version = (getINISetting f "Header" "version") as Number
			if version <= 0 or version > FILE_VERSION then (	
				messageBox (f + ": Incompatible version of Multiverse model set definition file.") beep:true
				return false
			)
			modelSet = #()
			numModels = (getINISetting f "ModelSet" "count") as number
			for i in 1 to numModels do (
	            local msf = getINISetting f "ModelSet" ("modelDefFile_" + i as string),
				      checked = getINISetting f "ModelSet" ("checked_" + i as string) == "true",
					  lodStatus_ = getINISetting f "ModelSet" ("LODStatus_" + i as string),
					  lodBaseMesh_ = getINISetting f "ModelSet" ("LODBaseMesh_" + i as string),
					  lodDistance_ = getINISetting f "ModelSet" ("LODDistance_" + i as string)
				if lodStatus_ == "" then lodStatus_ = "No LOD"
				lodDistance_ = if lodDistance_ == "" then 0 else lodDistance_ as number 
				append modelSet (MSE msf checked lodStatus_ lodBaseMesh_ lodDistance_)
			)
			modelSetChanged = false
		
			-- load into UI
			loadSetUI()
			
			-- success
			return true
		)
		
		-- load last-used set if any
		fn loadLastDef =
		(
			local lastDef = getINISetting configFile "Recent" "ModelSet" 
			if lastDef != "" and loadModelSet lastDef then (
				modelSetFile = lastDef 
				pickModelSet.text = fitButtonText modelSetFile 250; pickModelSet.toolTip = modelSetFile
			)
		)
		
		-- pick a new model set definition file, load into rollout
		on pickModelSet pressed do
		(
			if checkIfSetNeedsSave() != #cancel do (
				local f = getOpenFilename caption:"Model Set definition file" types:"Model Set Definition (*.mds)|*.mds|All(*.*)|*.*"
				if f != undefined then (
					if loadModelSet f then (
						modelSetFile = f
						pickModelSet.text = fitButtonText modelSetFile 250; pickModelSet.toolTip = modelSetFile 
						setINISetting configFile "Recent" "ModelSet" modelSetFile 
					)
				)
			)
		)
		
		-- new modelset def
		on newModelSetBtn pressed do
		(
			if checkIfSetNeedsSave() != #cancel do (
				local ync = yesNoCancelBox "Keep existing model set data?"
				if ync == #cancel then return false
				else if ync == #no then clearModelSet()
				modelSetFile = undefined
				pickModelSet.text = "Load..."; pickModelSet.toolTip = "Press to load a model set definition file"
				setINISetting configFile "Recent" "ModelSet" "" 
			)
		)
			
		-- save current modelset def
		on saveModelSetBtn pressed do
		(
			if modelSetFile == undefined then (
				-- file undefined, get save name
				modelSetFile = getSaveFileName types:"Model Set Definition (*.mds)|*.mds"
				if modelSetFile != undefined do (
					saveModelSet()
					pickModelSet.text = fitButtonText modelSetFile 250; pickModelSet.toolTip = modelSetFile
				)
			)
			else
				saveModelSet()
		)
		
		on saveAsModelSetBtn pressed do
		(
			newFile = getSaveFileName types:"Model Set Definition (*.mds)|*.mds"
			if newFile != undefined do (
				modelSetFile  = newFile
				saveModelSet()
				pickModelSet.text = fitButtonText modelSetFile 250; pickModelSet.toolTip = modelSetFile
			)
		)
		
		on addModel pressed do 
		(
			local f = getOpenFilename caption:"Model Definition file" types:"Model Definition (*.mdl)|*.mdl|All(*.*)|*.*"
			if f != undefined then (
				local newMS = MSE f true
				append modelSet newMS
				selectedModel = newMS
				loadSetUI()
				modelSetChanged = true
			)
		)
		
		-- delete selected component
		on delModel pressed do 
		(
			local li = models.selectedItem
			if li != undefined then (
				local i = li.index
				deleteItem modelSet i
				modelSetChanged = true
				loadSetUI()
			)
		)
		
		-- handle click on component listview entry
		on models itemClick i do
		(
			modelSelected()
		)
		
		-- handle check change on component listview entry
		on models itemCheck item do
		(
			if getINISetting modelSet[item.index].modelDefFile "Model" "baseName" == "" then item.checked = false
			else modelSet[item.index].checked = item.checked
			modelSetChanged = true
		)
		
		-- load selected model
		on loadModelBtn pressed do 
		(
			local li = models.selectedItem
			if li != undefined and checkIfNeedsSave() != #cancel do (
				local f = modelSet[li.index].modelDefFile
				if loadModel f then (
					modelFile = f
					MVE_Model.pickModel.text = fitButtonText modelFile 250; MVE_Model.pickModel.toolTip = modelFile
				)
			)
		)
		
		-- model def details items
		on lodStatus selected index do 
		(
			selectedModel.lodStatus = lodStatus.items[index]
			setUIStates()
			if selectedModel.lodStatus == "LOD Level" and selectedModel.lodBaseMesh == "" and lodBaseMesh.items.count > 0 then 
				selectedModel.lodBaseMesh = lodBaseMesh.items[lodBaseMesh.items.count]				
			modelSetChanged = true
			loadSetUI()
			setUIStates()
		)
		
		on lodBaseMesh selected index do
		(
			selectedModel.lodBaseMesh = lodBaseMesh.items[index]
			modelSetChanged = true
			loadSetUI()
			setUIStates()
		)
		
		on lodDistance changed val do
		(
			selectedModel.lodDistance = val
			loadSetUI()
			modelSetChanged = true
		)

		-- rollout open handler
		on MVE_ModelSet open do 
		(
			-- configure modelSet listview
			models.gridLines = true  
			models.View = #lvwReport  
			models.fullRowSelect = true 
			models.checkboxes = true
			models.labelEdit = #lvwManual
			cols = #(#("Model Name", 90), #("Model Definition File", 800))
			for i = 1 to cols.count do (
				models.columnHeaders.add text:cols[i][1]
				-- hack to set column width
				windows.sendMessage models.hwnd LV_SETCOLUMNWIDTH (i-1) cols[i][2]
			)
		)
	)

	-- dyanmically-selected model detail rollouts, based on model type & encoding
	rollout MVE_Static "Static Model Details"
	(
		editText staticBaseObjectName "Model Objects Name Prefix:" labelOnTop:true offset:[0,4]
		button loadModel "Load" align:#right offset:[0,-46]

		on staticBaseObjectName entered newName do 
		(
			m.staticBaseObjectName = newName
			MVE_Export.setUIStates()
			modelChanged = true
		)
		
		on loadModel pressed do 
		(
			if m.staticBaseObjectName != "" then
				MVE_Export.focusOn m.staticBaseObjectName
		)
	)
	
	-- the animated detail rollout is replicated for each encoding scheme, with common component list at the top
	--  and encoding-specific details after.  As the encoding scheme is changed, the correct rollout is added
	--  and the variable 'mro' points at the active animated-detail rollout
	rollout MVE_Animated_SF "Animated Model Components"  -- scene files
	(
		label L4 "Animation Components" 
		activeXControl components "MSComctlLib.ListViewCtrl" width:275 height:200 align:#center
		button addComponent "Add Animation" across:2
		button delComponent "Delete Animation" enabled:false
		
		group "Selected Animation"
		(
			editText animName "Animation Name: " 
			label L6 "Scene File:" align:#left  across:2 offset:[0,10]
			button loadAnim "Load" align:#right offset:[-4,4]
			button pickComponentFile "Choose..." width:250 offset:[0,-2]
		)
		
		group "Collision Volume Capsule"
		(
			label L7 "Associated Submesh:" align:#left across:2 offset:[0,3]
			pickButton pickCVSubMesh "Pick..." width:160 toolTip:"Pick submesh for CV object in scene." offset:[-10,0]
			checkbutton editCapsule "Edit CV Capsule" width:85 across:3
			spinner capsuleHeight "Height:" offset:[-8,3] range:[40,1000,120] width:95 enabled:false
			spinner capsuleRadius "Radius:" offset:[-3,3] range:[1,250,20] width:95 enabled:false
		)
		
		-- common control handlers
		on addComponent pressed do addComponent_pressed()
		on delComponent pressed do delComponent_pressed()
		on components itemClick i do components_itemClick ro:MVE_Animated_SF
		on components itemCheck item do components_itemCheck item 
		on animName entered newVal do animName_entered newVal
		on editCapsule changed state do editCapsule_changed state MVE_Animated_SF
		on capsuleRadius changed val do capsuleRadius_changed val MVE_Animated_SF 
		on capsuleHeight changed val do capsuleHeight_changed val 
		on pickCVSubMesh picked obj do pickCVSubMesh_picked obj MVE_Animated_SF
		
		-- handle press on component file button
		on pickComponentFile pressed do 
		(
			local f = getOpenFilename caption:"Component scene file" types:"Max Scene(*.max)|*.max|All(*.*)|*.*"
			if f != undefined and selectedComponent != undefined then (
				selectedComponent.fileName = f
				pickComponentFile.text = fitButtonText f 250; pickComponentFile.toolTip = f
				loadComponents()
				MVE_Export.setUIStates()
				setFocus animName
				modelChanged = true
			)
		)
		
		on loadAnim pressed do 
		(
			if selectedComponent != undefined and selectedComponent.fileName != ""  and checkForSave() then
				loadMaxFile selectedComponent.fileName useFileUnits:true
		)
		
		on MVE_Animated_SF open do componentRolloutOpen MVE_Animated_SF
	)
	
	rollout MVE_Animated_BF "Animated Model Components"  -- BIP files
	(
		fn bip_filt obj = classof obj == Biped_Object
		
		group "Base BIP details" 
		(
			label L1 "Biped Object:" align:#left
			pickButton pickBIPObject "Pick..." width:250 filter:bip_filt toolTip:"Pick model biped object in scene."
		)
		
		label L4 "Animation Components" 
		activeXControl components "MSComctlLib.ListViewCtrl" width:275 height:200 align:#center
		button addComponent "Add Animation" across:2 
		button delComponent "Delete Animation" enabled:false

		group "Selected Animation"
		(
			editText animName "Animaton Name: " 
			label L6 "BIP File:" align:#left across:3 offset:[0,8]
			button saveAnim "Save .bip" align:#right offset:[20,0]
			button loadAnim "Load .bip" align:#right offset:[-4,0]
			button pickComponentFile "Choose..." width:250  offset:[0,-2]
		)
		
		group "Collision Volume Capsule"
		(
			label L7 "Associated Submesh:" align:#left across:2 offset:[0,3]
			pickButton pickCVSubMesh "Pick..." width:160 toolTip:"Pick submesh for CV object in scene." offset:[-10,0]
			checkbutton editCapsule "Edit CV Capsule" width:85 across:3
			spinner capsuleHeight "Height:" offset:[-8,3] range:[40,1000,120] width:95 enabled:false
			spinner capsuleRadius "Radius:" offset:[-3,3] range:[1,250,20] width:95 enabled:false
		)
		
		-- common control handlers
		on addComponent pressed do addComponent_pressed()
		on delComponent pressed do delComponent_pressed()
		on components itemClick i do components_itemClick ro:MVE_Animated_BF
		on components itemCheck item do components_itemCheck item 
		on animName entered newVal do animName_entered newVal 
		on editCapsule changed state do editCapsule_changed state MVE_Animated_BF
		on capsuleRadius changed val do capsuleRadius_changed val MVE_Animated_BF 
		on capsuleHeight changed val do capsuleHeight_changed val 
		on pickCVSubMesh picked obj do pickCVSubMesh_picked obj MVE_Animated_BF
		
		-- handle bip base details
		on pickBIPObject picked node do 
		(
			m.bipObjectName = node.name
			pickBIPObject.text = fitButtonText node.name 250
			MVE_Export.setUIStates()
			modelChanged = true
		)
		
		-- handle press on component file button
		on pickComponentFile pressed do 
		(
			local f = getOpenFilename caption:"Component BIP file" types:"Biped Animation(*.bip)|*.bip|All(*.*)|*.*"
			if f != undefined and selectedComponent != undefined then (
				selectedComponent.fileName = f
				pickComponentFile.text = fitButtonText f 250; pickComponentFile.toolTip = f
				loadComponents()
				MVE_Export.setUIStates()
				setFocus animName
				modelChanged = true
			)
		)

		on loadAnim pressed do 
		(
			if selectedComponent != undefined and selectedComponent.fileName != "" then (
				local bipNode = getNodeByName m.bipObjectName, exception, saveRange
				if bipNode == undefined then
					messageBox ("Biped object not found: " + m.bipObjectName)
				else (
					try (
						saveRange = animationRange; animationRange = interval 0 1  -- force to short range so .bip file range is used
						biped.loadBIPFile bipNode.controller selectedComponent.fileName 
					)
					catch (
						exception = getCurrentException()
						saveRange = animationRange
					)
					if exception != undefined then 
						messageBox ("Error loading BIP file " + selectedComponent.fileName+ ":\n" + exception)
				)
			)
		)
		
		on saveAnim pressed do 
		(
			if selectedComponent != undefined and selectedComponent.fileName != "" then (
				local bipNode = getNodeByName m.bipObjectName, exception
				if bipNode == undefined then
					messageBox ("Biped object not found: " + m.bipObjectName)
				else (
					try
						biped.saveBIPFile bipNode.controller selectedComponent.fileName 
					catch
						exception = getCurrentException()
					if exception != undefined then
						messageBox ("Error saving BIP file " + selectedComponent.fileName+ ":\n" + exception)
				)
			)
		)
		
		on MVE_Animated_BF open do componentRolloutOpen MVE_Animated_BF
	)

	rollout MVE_Animated_SS "Animated Model Components"  -- shared scene
	(
		label L4 "Animation Components"
		activeXControl components "MSComctlLib.ListViewCtrl" width:275 height:200 align:#center
		button addComponent "Add Animation" across:2 
		button delComponent "Delete Animation" 

		group "Selected Animation"
		(
			editText animName "Animation Name: " 
			editText animBaseObjectName "Model Objects Name Prefix:" 
			button loadAnim "Load" align:#right offset:[0,-2]
		)
			
		group "Collision Volume Capsule"
		(
			label L7 "Associated Submesh:" align:#left across:2 offset:[0,3]
			pickButton pickCVSubMesh "Pick..." width:160 toolTip:"Pick submesh for CV object in scene." offset:[-10,0]
			checkbutton editCapsule "Edit CV Capsule" width:85 across:3
			spinner capsuleHeight "Height:" offset:[-8,3] range:[40,1000,120] width:95 enabled:false
			spinner capsuleRadius "Radius:" offset:[-3,3] range:[1,250,20] width:95 enabled:false
		)
		
		-- common control handlers
		on addComponent pressed do addComponent_pressed()
		on delComponent pressed do delComponent_pressed()
		on components itemClick i do components_itemClick()
		on components itemCheck item do components_itemCheck item 
		on animName entered newVal do animName_entered newVal 
		on editCapsule changed state do editCapsule_changed state MVE_Animated_SS
		on capsuleRadius changed val do capsuleRadius_changed val MVE_Animated_SS 
		on capsuleHeight changed val do capsuleHeight_changed val 
		on pickCVSubMesh picked obj do pickCVSubMesh_picked obj MVE_Animated_SS
		
		on MVE_Animated_SS open do componentRolloutOpen MVE_Animated_SS

		on animBaseObjectName entered val do 
		(
			if selectedComponent != undefined then (
				selectedComponent.baseObjectName = val
				loadComponents()
				modelChanged = true
			)
		)

		on loadAnim pressed do 
		(
			if selectedComponent != undefined and selectedComponent.baseObjectName != "" then
				MVE_Export.focusOn selectedComponent.baseObjectName 
		)
	)
		
		
	rollout MVE_Animated_FR "Animated Model Components"  -- frame range
	(
		label L4 "Animation Components"
		activeXControl components "MSComctlLib.ListViewCtrl" width:275 height:200 align:#center
		button addComponent "Add Animation" across:2
		button delComponent "Delete Animation" 

		group "Selected Animation"
		(
			editText animName "Animation Name: "
			spinner fromFrame "Start Frame: " across:2 type:#integer range:[0,1000,0]
			spinner toFrame "End Frame:" type:#integer range:[0,1000,0]
			button loadAnim "Load" align:#right
		)
		
		group "Collision Volume Capsule"
		(
			label L7 "Associated Submesh:" align:#left across:2 offset:[0,3]
			pickButton pickCVSubMesh "Pick..." width:160 toolTip:"Pick submesh for CV object in scene." offset:[-10,0]
			checkbutton editCapsule "Edit CV Capsule" width:85 across:3
			spinner capsuleHeight "Height:" offset:[-8,3] range:[40,1000,120] width:95 enabled:false
			spinner capsuleRadius "Radius:" offset:[-3,3] range:[1,250,20] width:95 enabled:false
		)
		
		-- common control handlers
		on addComponent pressed do addComponent_pressed()
		on delComponent pressed do delComponent_pressed()
		on components itemClick i do components_itemClick()
		on components itemCheck item do components_itemCheck item 
		on animName entered newVal do animName_entered newVal 
		on editCapsule changed state do editCapsule_changed state MVE_Animated_FR
		on capsuleRadius changed val do capsuleRadius_changed val MVE_Animated_FR 
		on capsuleHeight changed val do capsuleHeight_changed val 
		on pickCVSubMesh picked obj do pickCVSubMesh_picked obj MVE_Animated_FR
		
		on fromFrame changed val do 
		(
			if selectedComponent != undefined then (
				selectedComponent.startFrame = val
				loadComponents()
				modelChanged = true
			)
		)
	
		on toFrame changed val do 
		(
			if selectedComponent != undefined then (
				selectedComponent.endFrame = val
				loadComponents()
				modelChanged = true
			)
		)

		on loadAnim pressed do 
		(
			if selectedComponent != undefined and selectedComponent.startFrame != selectedComponent.endFrame then
				animationRange = Interval selectedComponent.startFrame selectedComponent.endFrame
		)

		on MVE_Animated_FR open do componentRolloutOpen MVE_Animated_FR
	)
		
	-- defines a model object
	rollout MVE_Model "Model Definitions"
	(
		label L0 "Model Definition File:" align:#left
		button pickModel "Load..." width:250
		button newModelBtn "Reset" across:3 offset:[63,0] width:55
		button saveModelBtn "Save" offset:[37,0] width:55
		button saveAsModelBtn "Save As" offset:[12,0] width:55
	
		group "Model Basics"
		(
			editText baseName "Base Name:"
			label L1 "Base MAX Scene File:" align:#left  across:2 offset:[0,10]
			button loadScene "Load Scene" align:#right offset:[-4,4]
			button pickBaseScene "Choose..." width:250 offset:[0,-2]
			label L2 "Type:" across:2 align:#left offset:[30,4]
			dropDownList modelType offset:[-60,0] items:#("Static", "Animated", "Morph <TBD>")
			label L3 "Encoding:" across:2  align:#left offset:[10,4]
			dropDownList encoding offset:[-60,0] items:#("Single Scene File", "Shared Scene File")
		)
		
		-- load last-used model if any
		fn loadLastDef =
		(
			local lastDef = getINISetting configFile "Recent" "ModelDef" 
			if lastDef != "" and loadModel lastDef then (
				modelFile = lastDef 
				pickModel.text = fitButtonText modelFile 250; pickModel.toolTip = modelFile 
			)
		)
		
		/* ------ MVE_Model rollout UI handers ------------- */
		
		-- pick a new model definition file, load into rollout
		on pickModel pressed do
		(
			if checkIfNeedsSave() != #cancel do (
				local f = getOpenFilename caption:"Model definition file" types:"Model Definition (*.mdl)|*.mdl|All(*.*)|*.*"
				if f != undefined then (
					if loadModel f then (
						modelFile = f
						pickModel.text = fitButtonText modelFile 250; pickModel.toolTip = modelFile
						setINISetting configFile "Recent" "ModelDef" modelFile 
					)
				)
			)
		)
		
		-- new model def
		on newModelBtn pressed do
		(
			if checkIfNeedsSave() != #cancel do (
				local ync = yesNoCancelBox "Keep existing model definition data?"
				if ync == #cancel then return false
				else if ync == #no then clearModel()
				-- clear modelFile
				modelFile = undefined
				pickModel.text = "Load..."; pickModel.toolTip = "Press to load a model definition file"
				setINISetting configFile "Recent" "ModelDef" ""
			)
		)
			
		-- save current model def
		on saveModelBtn pressed do
		(
			if modelFile == undefined then (
				-- file undefined, get save name
				modelFile = getSaveFileName types:"Model Definition (*.mdl)|*.mdl"
				if modelFile != undefined do (
					saveModel()
					pickModel.text = fitButtonText modelFile 250; pickModel.toolTip = modelFile
				)
			)
			else
				saveModel()
		)
		
		on saveAsModelBtn pressed do
		(
			local newFile = getSaveFileName types:"Model Definition (*.mdl)|*.mdl"
			if newFile != undefined do (
				modelFile = newFile
				saveModel()
				pickModel.text = fitButtonText modelFile 250; pickModel.toolTip = modelFile
			)
		)
	
		-- handle model basics entries
		on baseName entered newName do 
		(
			m.baseName = newName
			MVE_Export.setUIStates()
			modelChanged = true
		)
		
		on pickBaseScene pressed do 
		(
			local dfltFile = if m.baseScene == "" then maxFilePath + maxFileName else ""
			local f = getOpenFilename caption:"Model base scene file" filename:dfltFile types:"Max Scene(*.max)|*.max|All(*.*)|*.*"
			if f != undefined then (
				m.baseScene = f
				pickBaseScene.text = fitButtonText f 250; pickBaseScene.toolTip = f
				MVE_Export.setUIStates()
				modelChanged = true
			)
		)
		
		on loadScene pressed do 
		(
			if m.baseScene != "" and checkForSave() then
				loadMaxFile m.baseScene useFileUnits:true
		)
		
		on modelType selected index do 
		(
			m.modelType = modelType.selected
			setModelEncodings()
			m.encoding = encoding.selected
			openRollouts()
			setUIStates()
			loadUI()
			modelChanged = true
		)
		
		on encoding selected index do
		(
			m.encoding = encoding.selected
			openRollouts()
			setUIStates()
			loadUI()
			modelChanged = true
		)
	)
	
	-- defines the selected model's target repositories
	rollout MVE_Repositories "Asset Repositories"
	(
		group "Asset Info"
		(
			label L1 "Category:" across:2  align:#left offset:[20,2]
			edittext assetCategory offset:[-54,2] width:143
			dropdownList assetCategoryDD align:#center offset:[20,-3] width:140  items:#("Type in or Select:", "Buildings", "Props", "Walls", "Characters", "Creatures")
			--combobox assetCategory align:#left width:140 offset:[-60,0] items:#("Buildings", "Props", "Walls", "Characters", "Creatures") height:7
			editText description "Description:" labelOnTop:true 
		)
		label L2 "Target Repositories:"
		activeXControl repositories "MSComctlLib.ListViewCtrl" width:275 height:100 align:#center
		button addRepository "Add Repository" across:2
		button delRepository "Delete Repository"
		
		group "Selected Repository Details"
		(
			label L3 "Repository Root Directory:" align:#left
			button pickRootDir "Choose..." width:250
		)
		
		-- load repositories into UI
		fn loadRepositories =
		(
			repositories.listItems.clear()
			
			-- add repositories
			for r in m.repositories do (
				local li = repositories.listItems.add text:(" " + r.rootDirectory)
				li.checked = r.checked
				if r == selectedRepository do li.selected = true
			)
			repositories.enabled = true
			MVE_Export.setUIStates()
			repositories.dropHighlight = repositories.selectedItem
			if repositories.selectedItem != undefined then repositories.selectedItem.ensureVisible()
			L2.text = "Target Repositories [" + m.repositories.count as string + "]"
		)
		
		-- handle component selection
		fn repositorySelected = 
		(
			repositories.dropHighlight = undefined
			local li = repositories.selectedItem, d = "", tt = "Click to pick root directory."
			if li != undefined then (
				selectedRepository = m.repositories [li.index]
				d = fitButtonText selectedRepository.rootDirectory 250; pickRootDir.toolTip = d
				pickRootDir.enabled = true
				repositories.dropHighlight = li
			)
			else (
				selectedRepository = undefined
				pickRootDir.enabled = false
			)
			pickRootDir.text = if d == "" then "Choose..." else d; pickRootDir.toolTip = tt
		)
				
		--  add new repository
		on addRepository pressed do 
		(
			append m.repositories (Repository())
			loadRepositories()
			repositories.listItems[repositories.listItems.count].selected = true
			repositorySelected()
			pickRootDir.pressed()
			repositories.dropHighlight = repositories.selectedItem
			repositories.selectedItem.ensureVisible()
			modelChanged = true
		)
		
		-- delete selected repository
		on delRepository pressed do 
		(
			local li = repositories.selectedItem
			if li != undefined then (
				local i = li.index
				deleteItem m.repositories i
				loadRepositories()
				-- select next repos in list or end of list if last
				if i > m.repositories.count then i = m.repositories.count
				if i > 0 then repositories.listItems[i].selected = true
				repositorySelected()
				modelChanged = true
			)
			MVE_Export.setUIStates()
		)
		
		-- handle click on repository listview entry
		on repositories itemClick i do
		(
			repositorySelected()
			-- short-cut to file name picker if name is "Choose..." when selected
			if selectedRepository!= undefined and selectedRepository.rootDirectory == "Choose..." then
				MVE_Repositories.pickRootDir.pressed()
		)
		
		-- handle check change on repository listview entry
		on repositories itemCheck item do
		(
			m.repositories[item.index].checked = item.checked
			modelChanged = true
		)
		
		-- handle new model asset type & description
		on description entered newVal do (m.assetDescription = newVal; modelChanged = true )
		on assetCategory entered v do (m.assetCategory = assetCategory.text; modelChanged = true )
		on assetCategoryDD selected index do (m.assetCategory = assetCategory.text = assetCategoryDD.selected; modelChanged = true )
		--on assetCategory entered v do m.assetCategory = assetCategory.text
		--on assetCategory selected index do m.assetCategory = assetCategory.selected
		
		-- handle press on repository base directory button
		on pickRootDir pressed do 
		(
			local d = getSavePath caption:"Repository Root Directory"
			if d != undefined and selectedRepository != undefined then (
				selectedRepository.rootDirectory = d
				pickRootDir.text = fitButtonText d 250; pickRootDir.toolTip = d
				loadRepositories()
				modelChanged = true
			)
		)
		
		on MVE_Repositories open do 
		(
			-- configure listview AXcontrol
			repositories.gridLines = true  
			repositories.View = #lvwReport  
			repositories.fullRowSelect = true 
			repositories.checkboxes = true
			repositories.labelEdit = #lvwManual
			cols = #(#("      Root Directory", 800))
			for i = 1 to cols.count do (
				repositories.columnHeaders.add text:cols[i][1]
				windows.sendMessage repositories.hwnd LV_SETCOLUMNWIDTH (i-1) cols[i][2]
			)
		)
	)
	
	-- performs & controls export
	rollout MVE_Export "Export"
	(
		local exportDirectory, currentComponent, exportModel, baseComponent, 
			  statusMode = #message, logFile, showStatus, logNo, prepLODProcessing, selectedLODSets 
		
		group "Batch Setup"
		(
			label L1 "Export:" across:2  align:#left offset:[30,4]
			dropdownList exportType offset:[-30,0] items:#("Current Scene", "Current Model", "Checked Models", "All Models") selection:2 width:120
			label L2a "Animations:" across:2  align:#left offset:[30,4] enabled:false
			dropdownList componentChoice offset:[-30,0] items:#("Current Animation", "Checked", "All") selection:2 width:120 enabled:false
			label L3 "Repositories:" across:2  align:#left offset:[30,4]
			dropdownList repositoryChoice offset:[-30,0] items:#("Checked", "All") width:120
		)
		
		group "Export Steps"
		(
				
			checkbox enableColladaExport "Collada Export" offset:[70,0] checked:true
			checkbox enableModelConversion "Model Conversion" offset:[70,0] checked:true
			checkbox enableRepositoryImport "Repository Import" offset:[70,0] checked:true
			checkbox preserveDotMaterial "Preserve existing .material" offset:[100,0] checked:true
			checkbox enableOpenInViewer "Open in Viewer" offset:[70,0] checked:false
		)
		
		group "Export Options"
		(
			checkbox enableLODProcessing "Merge Manual LOD Meshes" checked:false
			checkbox enableFixDotMaterial "Update .material file from derived textures" checked:true
			checkbox enableImportTextures "Import derived textures" checked:true
			checkbox enableSaveTextures "Backup derived textures in Export directory" checked:true
			checkbox enableAddPhysics "Generate .physics from Collision Capsule" checked:true --across:2 align:#left
			--dropdownList addPhysics items:#("Capsule for All Submeshes", "Capsule for Largest Submesh") width:135 offset:[-7,-2]
			checkbox enableSequenceFolders "Export to sequential folders" checked:true --across:2 align:#left
		)
		
		group "Pre-Export Checks"
		(
			checkbutton checkSkinBoneCountBtn "Check Bone Limits on Skinned Meshes"
		)
		
		group "Export Log"
		(
			checkbox keepLog "Log Export Actions" checked:true across:3
			button showLogBtn "Show Log" offset:[36,-2]
			button clearLogBtn "Clear Log" offset:[16,-2]
		)
		
		button export "Export" width:150 height:35 enabled:false offset:[0,5]
		label statusMsg "" align:#center
		checkbutton statusWarn "" highlightColor:(color 255 128 128) offset:[0,-18] visible:false
		on statusWarn changed state do status.checked = statusMode == #warning  -- keep status button in warning state
		
		on enableRepositoryImport changed val do enableImportTextures.enabled = enableRepositoryImport.checked
		on enableImportTextures changed val do enableSaveTextures.enabled = enableImportTextures.enabled and enableImportTextures.checked
		
		fn findComponent components name = 
		(
			for c in components do if stricmp name c.animName == 0 then return c
			return undefined
		)
		
		-- export log utilities
		fn openLog label:"Export Started" =
		(
			if keepLog.checked then (
				if doesFileExist logFileName then logFile = openFile logFileName mode:"at"
				else logFile = createFile logFileName
				format "\n** ----------------- % % ------------------------**\n\n" label localTime to:logFile
			)
		)
		
		fn closeLog = 
		(
			if keepLog.checked then (gc(); close logFile)
		)
		
		fn showLog = 
		(
			if doesFileExist logFileName then DOSCommand ("start /B notepad.exe " + logFileName)
		)
			   
		fn clearLog = 
		(
			if classOf logFile == FileStream then close logFile
			deleteFile logFileName
		)
		
		fn log msg = 
		(
			if keepLog.checked then format "%: %\n" localTime msg to:logFile
			-- showStatus msg
 		)
		
		-- status message displays
		fn showStatus msg =
		(
			statusWarn.visible = false
			if msg == "" then statusMsg.visible = false
			else (
				statusMsg.visible = true
				statusMsg.text = "Status: " + msg
				statusMode = #message
			)
		)
		
		fn showWarning msg = 
		(
			statusMsg.visible = false
			statusWarn.visible = true
			statusWarn.checked = true
			statusWarn.text = msg
			statusMode = #warning
		)
		
		fn to000String val = 
		(
			local str = ""
		    for i in 2 to 1 by -1 do str += ((val / 10 ^ i) as integer) as string
			str += ((mod val 10) as integer) as string
			return str 
		)
		
		local compChoiceItems1 = #("Current Animation", "Checked", "All"),
		      compChoiceItems2 = #("Checked", "All")
			  
		-- sets UI control states for export rollout & does various model validity checks
		--  also sets currentComponent based on model & export type selections
		fn setUIStates = 
		(
			currentComponent = undefined
			export.text = ""; showStatus ""; export.enabled = true
			enableImportTextures.enabled = enableRepositoryImport.checked
			enableSaveTextures.enabled = enableImportTextures.enabled and enableImportTextures.checked
			if m.modelType == "Static" then (
				componentChoice.enabled = L2a.enabled = false; enableAddPhysics.enabled = false
				componentChoice.items = #()
				if m.encoding == "Shared Scene File" and m.staticBaseObjectName == "" then (
					showWarning "Cannot export: model has no base object name"
					export.enabled = false
				)
			)
			else if m.modelType == "Animated" then (
				componentChoice.enabled = L2a.enabled = true
				case exportType.selected of (
					"Current Scene":
						if m.encoding == "Scene Files" then (
							-- must be base file or in component files, use matching component if so, components menu disabled
							currentComponent = undefined
							if stricmp m.baseScene (maxFilePath + maxFileName) != 0 then (
								for c in m.components do
									if stricmp c.fileName (maxFilePath + maxFileName) == 0 then (
										-- found component matching current MAX scene
										currentComponent = c
										if c.animName == "" then (
											showWarning "Cannot export: scene's component does not have a name"
											export.enabled = false
										)
										else 
											export.text = "Export " + m.baseName + ": " + c.animName
										exit
									)
								if currentComponent == undefined then (
									showWarning "Cannot export: scene is not model base or component"
									export.enabled = false
								)
							)
							componentChoice.enabled = L2a.enabled = false
							componentChoice.items = #()
						)
						else (
							-- model base must match current scene, then components menu = Current Animation, checked, all
							componentChoice.items = compChoiceItems1 
							if stricmp m.baseScene (maxFilePath + maxFileName) == 0 then (
								showWarning "* Current scene does not match model base scene *"
								export.enabled = false
							) 
							else
								export.text = "Export " + m.baseName
						)
						
					"Current Model":
						componentChoice.items = compChoiceItems1 
						
					"All Models":
						componentChoice.items = compChoiceItems2
						
					"Checked Models":
						componentChoice.items = compChoiceItems2
				)
			)
			
			-- common checks for current scene/mode
			enableLODProcessing.enabled = modelSet != undefined and modelSet.count > 0 
			if exportType.selected == "All Models" or exportType.selected == "Checked Models" then (
				enableOpenInViewer.enabled = false
				export.text = "Export " + exportType.selected 
				if modelSet == undefined or modelSet.count == 0 then (
					export.enabled = false
					showWarning "Cannot export: Model Set not defined or empty"
				)
			)
			else (
				enableOpenInViewer.enabled = true
				if m.repositories.count == 0 then (
					showWarning "Cannot export: model has no repositories defined"
					export.enabled = false
				)
				else if m.baseName == "" then (
					showWarning "Cannot export: model does not have a base name"
					export.enabled = false
				)
				else if m.baseScene == "" then (
					showWarning "Cannot export: base scene has not been chosen"
					export.enabled = false
				)
				else (
					if export.text == "" then export.text = "Export " + m.baseName
					if m.modelType == "Animated" then (
						if componentChoice.selected == "Current Animation" then (
							-- component must be selected
							if selectedComponent == undefined then (
								showWarning "Cannot export: animation component not selected"
								export.enabled = false
							)
							else if selectedComponent.animName == "" then (
								showWarning "Cannot export: animation component does not have a name"
								export.enabled = false
							)
							else 
								export.text = "Export " + m.baseName + ": " + selectedComponent.animName
							currentComponent = selectedComponent
						)
						else if m.encoding != "Scene Files" then (
							-- check for 'base' animation
							if findComponent m.components "base" == undefined then (
								showWarning "Cannot export: 'base' animation component is missing"
								export.enabled = false
							)
						)
					)
				)
			)
			if export.text == "" then export.text = "Export"
		)
		
		-- validate model meshes
		fn checkSkinBoneCount =
		(
			checkSkinBoneCountBtn.checked = true
			setWaitCursor()
			openLog label:"Mesh Validity Check"
			log "  Checking bone-per-vertex counts on skinned meshes"
			local badNodes = #()
			max modify mode
			for g in geometry where classOf g == Editable_mesh do (
				for m in g.modifiers where classOf m == Skin do (
					log ("    Checking " + g.name)
					select g
					local badVerts =
						for i in 1 to g.numVerts where skinOps.getVertexWeightCount m i > 4 collect i 
					if badVerts.count > 0 then (
						append badNodes g
						log ("      ** Bones-per-vertex limit(4) exceeded on " + g.name + ", vertices " + badVerts as string)
					)
				)
			)
			setArrowCursor()
			if badNodes.count > 0 then (
				select badNodes
				local badNodeNames = "", sep = ""
				for n in badNodes do (badNodeNames += sep + n.name; sep = ", ")
				messageBox ("Bone-per-vertex limit(4) exceeded on following meshes:\n\n    " + 
							badNodeNames + "\n\nSee export log for more details.") beep:true
			)
			checkSkinBoneCountBtn.checked = false
			closeLog()
		)
		
		-- make model geometry containing baseObjectName the focus by unhiding it and hiding others
		--  returns those geom objects that were hidden prior to focus for easy state restoration
		--  note: potential model components are scene geometry and scene helpers, anything else?
		fn focusOn baseObjectName = 
		(
			local modelObjects = join (geometry as array) (helpers as array),  
			      hidden = for o in modelObjects where o.isHidden collect o
			modelObjects.isHidden = true
			for o in modelObjects where matchPattern o.name pattern:("*" + baseObjectName + "*") ignoreCase:true do
				o.isHidden = false
			return hidden
		)
		
		-- fix .material file for exportModel, return material name list
		fn fixDotMaterial dotMatFile =
		(
			log ("  Deriving textures for " + getFilenameFile dotMatFile + ".material")
			-- open the .material file & create a new output tmp
			local src = openFile dotMatFile,
				  mtls = #()
			if src != undefined then (
				local tmpFile = dotMatFile + "_tmp"
				deleteFile tmpFile
				local dst = createFile tmpFile
				if dst != undefined then (
					local mtlFileName
					-- loop over lines in .material file looking for textures to fixup
					while not eof src do (
						local line = readLine src
						if matchPattern line pattern:"material *" then (
							-- found a material header, pick up mtl name & look for a scene material of that name
							local i = findString line "."
							if i != undefined then (
								local mtlName = substring line (i+1) -1
								local mtl = sceneMaterials[mtlName]
								if mtl != undefined then (
									-- found named scene material, look for diffuseMap bitmap filename
									if mtl.diffuseMap != undefined and
									   classOf mtl.diffuseMap == Bitmaptexture then
									(
										local mapFile = mtl.diffuseMap.filename
										if mapFile != undefined then (
											-- found a bitmap tex file, record name
											append mtls mapFile
											mtlFileName = (getFileNameFile mapFile) + (getFileNameType mapFile)
											log ("    found texture " + mtlFileName + " in material " + mtlName)
										)
									)
								)
							)
						)
						else if matchPattern line pattern:"*texture " and mtlFileName != undefined then (
							line += mtlFileName
							mtlFileName = undefined
						)
						else if matchPattern line pattern:"*specular 0.90000 0.90000 0.90000*" then (
							local specFix = "specular 0.10000 0.10000 0.10000"
							log "    replacing default specularity"
							log ("    orig line: " + line)
							line = replace line (findString line "specular") specFix.count specFix
							log ("    new line: " + line)
						)
						-- write line
						format "%\n" line to:dst
					)
					
					close src
					close dst
					-- canbe called for either .material fixup or texture import, only rename updated .material if enabled
					if enableFixDotMaterial.checked then (
						-- rename fixed tmp to .material
						deleteFile (dotMatFile + "-orig")
						renameFile dotMatFile (dotMatFile + "-orig")
						renameFile tmpFile dotMatFile 
					)
					else
						deleteFile tmpFile
				)
			)
			else
				close src
			return mtls
		)
		
		-- export scene to DAE for [currentComponent of] exportModel
		fn colladaExport scene exportDirectory firstExportDir =
		(
			if enableColladaExport.checked then (
				local daeFile = exportDirectory + exportModel.baseName +
							(if currentComponent != undefined then "_" + currentComponent.animName else "") + ".DAE",
				      saveHidden, saveRange
					  
				log ("Collada export to " + daeFile)
			
				-- only need to run export for first repository
				if firstExportDir == undefined then (
				
					-- load scene if needed
					log ("  Base scene " + scene)
					if stricmp scene (maxFilePath + maxFileName) != 0 then (
						if not checkForSave() then throw "Export cancelled."
						log "    loading scene"
						if not loadMaxFile scene useFileUnits:true then
							throw ("Unable to load scene file: " + scene)
					)
					
					-- setup scene state for export based on model type and encoding
					if exportModel.modelType == "Static" and exportModel.encoding == "Shared Scene File" then (
						-- hide all except current model's geom
						saveHidden = focusOn exportModel.staticBaseObjectName
						log ("  Selecting objects with prefix " + exportModel.staticBaseObjectName)
					)
					else if exportModel.modelType == "Animated" and exportModel.encoding != "Scene Files" then (
						-- if currentComponent not defined use 'base' animation
						local c = currentComponent
						if c == undefined then (
							c = findComponent exportModel.components "base"
							if c == undefined then
								throw ("Model " + exportModel.baseName + "has no 'base' animation component.")
						)
						log ("  Exporting " + c.animName + " animation")
						
						case exportModel.encoding of (
							"BIP Files": (
								log ("  Loading BIP file " + c.fileName + " for biped " + exportModel.bipObjectName)
								local bipNode = getNodeByName exportModel.bipObjectName, exception
								if bipNode == undefined then throw ("Biped object not found: " + exportModel.bipObjectName)
								try (
									saveRange = animationRange; animationRange = interval 0 1  -- force to short range so .bip file range is used
									biped.loadBIPFile bipNode.controller c.fileName
								)
								catch (
									exception = getCurrentException()
									animationRange = saveRange
								)
								if exception != undefined then throw ("Error loading BIP file " + c.fileName+ ": " + exception)
							)
							"Shared Scene": (
								log ("  Select objects with prefix " + c.baseObjectName)
								saveHidden = focusOn c.baseObjectName
							)
							"Frame Range": (
								saveRange = animationRange
								animationRange = Interval c.startFrame c.endFrame
								log ("  Setting animation range to " + animationRange as string)
							)
						)
					)
					
					-- move aside existing Collada config .ini, make one tuned for TMN
					local maxver = (maxversion())[1],
					      iniFile = if maxver >= 9000 then "ColladaMax" else "ColladaExporter"
					local colladaINIFile = (GetDir #plugcfg) + "\\" + iniFile + ".ini"
					local colladaINIFileBackup = (GetDir #plugcfg) + "\\" + iniFile + "Backup.ini"
					deleteFile colladaINIFileBackup
					local restoreBackup = false
					if doesFileExist colladaINIFile then (
						if renameFile colladaINIFile colladaINIFileBackup != true then throw "Unable to backup Collada configuration"
						restoreBackup = true
					)

					local filestream = createFile colladaINIFile
					if maxVer >= 9000 then 
						format "[ColladaMax]\n" to: filestream
					format "%=%\n" "normals" "1" to: filestream
					format "%=%\n" "triangulate" "1" to: filestream
					format "%=%\n" "xrefs" "1" to: filestream
					format "%=%\n" "tangents" "0" to: filestream
					format "%=%\n" "animations" "1" to: filestream
					if exportModel.encoding == "Frame Range" then
						format "%=%\n" "sampleAnim" "1" to: filestream
					else
						format "%=%\n" "sampleAnim" "0" to: filestream
					format "%=%\n" "createClip" "1" to: filestream
					format "%=%\n" "bakeMatrices" "1" to: filestream
					format "%=%\n" "relativePaths" "1" to: filestream
					format "%=%\n" "animStart" "0" to: filestream
					format "%=%\n" "animEnd" "5" to: filestream
					close filestream
				
					-- the Collada export
					try (
						showStatus "Exporting .DAE..."
						exportFile daeFile #noPrompt    -- HEY!! ensure collada export INI file has correct settings
						log "  Collada export complete"
					)
					catch (
						deleteFile colladaINIFile
						renameFile colladaINIFileBackup colladaINIFile
						throw()
					)
					
					-- restore .ini file
					deleteFile colladaINIFile
					if restoreBackup then (
						if renameFile colladaINIFileBackup colladaINIFile != true then throw "Unable to restore Collada configuration"
					)
					
				)
				else (
					-- already exported, just copy across from first repos
				    local firstDAEFile = firstExportDir + exportModel.baseName + ".DAE"
					deleteFile daeFile
					copyFile firstDAEFile daeFile
					log ("  Copying first export " + firstDAEFile + " to " + daeFile)
				)
				
				-- restore scene state
				log "  Restoring scene state"
				if saveHidden != undefined then (
					local modelObjects = join (geometry as array) (helpers as array)
					modelObjects.isHidden = false
					saveHidden.isHidden = true
				)
				if saveRange != undefined then
					animationRange = saveRange
			)
		)
		
		-- runs the Multiverse conversion tool using given args, waits for waitFile to appear
		fn runConversionTool args waitFile exportDirectory =
		(
			local cmd = "start /b /wait /d\"" + converterPath + "\" " + converterExe + " ",
			      logName = "ConversionTool_" + to000String (logNo += 1) + ".log",
				  commonSwitches = "",
				  maxver = (maxversion())[1]

			if maxver >= 9000 then 
			      commonSwitches = " --3ds --dst_dir " + exportDirectory + " --src_dir " + exportDirectory + 
				  					" --log_file " + exportDirectory + logName + " "
			else
			      commonSwitches = " --3ds --dst_dir " + exportDirectory + " --src_dir " + exportDirectory + 
				  					" --log_file " + exportDirectory + logName + " "
						
			if waitFile != "" then (
				-- temporarily rename any existing output wait-file so we can see when new one turns up
				waitFile = exportDirectory + waitFile
				local oldWaitFile = waitFile + "-old"
				deleteFile oldWaitFile 
				renameFile waitFile oldWaitFile 
			)
			
			-- write our arguments to the export file
            local argsFileName = exportDirectory + "export_args"
			if doesFileExist argsFileName then deleteFile argsFileName
			argsFile = createFile argsFileName
			format "% %" commonSwitches args to:argsFile
			close argsFile
			
			-- launch converter
			log ("  Running ModelConverter command: " + "start /d" + converterPath + " " + converterExe + " " + "--args_file " + argsFileName)
			log ("     Log file: " + logName)
			log ("     Converter args:")
			log ("        " + commonSwitches + " " + args)
			showStatus "Running Conversion Tool..."
			DOSCommand (cmd + "--args_file " + argsFileName)

			-- if expect output file doesn't exist, complain.
			if waitFile != "" and not doesFileExist waitFile then
				throw "Model Conversion failed, cannot continue with export."
			
			/*
				-- ** note the following may be obsoleted by the /wait on the DOS command, still testing
				-- watch for waitFile file to appear in sleep loop  (HEY!! maybe need converter to place an all-done file for us)
				log ("  Waiting for output file " + waitFile)
				local sleepCount = 0
				while not doesFileExist waitFile and sleepCount < toolTimeout do (
					sleep 1
					sleepCount += 1
	--format "waiting %\n" sleepCount
				)
				if not doesFileExist waitFile then (
					-- timed-out, restore old wait file & bail
					renameFile oldWaitFile waitFile 
					throw "Model Conversion timed out, cannot continue with export."
				)
				-- safety sleep  
				sleep 1
			*/
			log "  ModelConverter command finished"
		)

		local physicsTemplate =  
"<physics_model>
  <rigid_body sid=\"%\">
    <technique_common>
      <shape>
        <translate>% % %</translate>
        <capsule>
          <height>%</height>
          <radius>% %</radius>
        </capsule>
      </shape>
    </technique_common>
  </rigid_body>
</physics_model>
"
		
		-- build optional .physics file for current export model
		fn buildDefaultPhysics exportDirectory = 
		(
			if enableAddPhysics.enabled and enableAddPhysics.checked then (
				physicsFile = exportDirectory + exportModel.baseName + ".physics"
				log ("    building default .physics file " + physicsFile)
				deleteFile physicsFile 
				local pf = createFile physicsFile,
				      toMeters = case units.systemType of (
					  	  #Inches: 0.0254
						  #Feet: 0.3048
						  #Miles: 1609.344
						  #Millimeters: 0.001
						  #Centimeters: 0.01
						  #Meters: 1
						  #Kilometers: 1000)
				local centerHeight = (exportModel.cvPos.z + exportModel.cvHeight / 2) * toMeters,
				      length = (exportModel.cvHeight - 2 * exportModel.cvRadius) * toMeters,
					  radius = exportModel.cvRadius * toMeters,
					  pos = exportModel.cvPos * toMeters,
					  maxver = (maxversion())[1]
				if maxver >= 9000 then
					format physicsTemplate (exportModel.cvBaseMeshName + "-lib.0") pos.x centerHeight (-pos.y) \
					                       length radius radius to:pf 
				else
					format physicsTemplate (exportModel.cvBaseMeshName + "-mesh.0") pos.x centerHeight (-pos.y) \
					                       length radius radius to:pf 
				close pf
			)
		)
		
		-- model conversion for exportModel (& currentComponent)
		fn modelConversion exportDirectory firstExportDir = 
		(
			if enableModelConversion.checked then (
				local baseDAEFile = exportModel.baseName + ".DAE",
					  meshFile = exportModel.baseName + ".mesh",
					  baseSkeletonFile = exportModel.baseName + "_base.skeleton",
					  baseSkeletonPath = exportDirectory + baseSkeletonFile,
					  outSkeleton = exportModel.baseName + ".skeleton",
					  args = baseDAEFile 
					  
				log ("Model conversion for " + exportModel.baseName)
				-- only need to run export for first repository
				if firstExportDir == undefined then (

					-- static or animated conversion?
					if exportModel.modelType == "Static" then (
						buildDefaultPhysics exportDirectory
						runConversionTool args meshFile exportDirectory
						-- fix .material file & gather found textures for import step
						if enableFixDotMaterial.checked or enableImportTextures.checked do
							exportModel.textures = fixDotMaterial (exportDirectory + exportModel.baseName + ".material")
					)
					
					else if exportModel.modelType == "Animated" then (
						-- base mesh/skelton conversion first, if needed/requested
						if currentComponent == undefined or 
							not doesFileExist baseSkeletonPath or
							componentChoice.selected != "Current Animation" then
						(
							buildDefaultPhysics exportDirectory
							if not doesFileExist (exportDirectory + baseDAEFile) then
								throw ("Base .DAE file missing: " + baseDAEFile + ". Need to perform base Collada export.")
							log ("  Base mesh/skeleton conversion: " + baseDAEFile + " to " + meshFile + " and " + baseSkeletonFile)
							runConversionTool args meshFile exportDirectory
							deleteFile baseSkeletonPath 
							renameFile (exportDirectory + outSkeleton) baseSkeletonPath 
							-- fix .material file & gather found textures for import step
							if enableFixDotMaterial.checked or enableImportTextures.checked do
								exportModel.textures = fixDotMaterial (exportDirectory + exportModel.baseName + ".material")
						)
						-- skeleton animation merge
						--  for now, even in the presence of explicit component selection, the merge will try to merge
						--  all known components for which DAE files exist, mostly so you can do incremental work
						--  which merges into current full asset.  This probably needs rethinking.
						-- note we skip 'base' animation component here, should already in base skeleton
						log ("  Merge animations into " + outSkeleton)
						if not doesFileExist baseSkeletonPath then
							throw ("Base skeleton file missing: " + baseSkeletonFile)
						args = "--base_skeleton " + baseSkeletonFile
						for c in exportModel.components where c.animName != "base" do (
							local daeName = exportModel.baseName + "_" + c.animName + ".DAE"
							if doesFileExist (exportDirectory + daeName) then (
								args += " --animation " + c.animName + " " + daeName
								log ("    include animation " + c.animName + " from file " + daeName)
							)
						)
						args += " --out_skeleton " + outSkeleton 
						runConversionTool args outSkeleton exportDirectory 
					)
				)
				else (
					-- already run conversion into firstExportDir, just copy results there to exp. dir.
					log ("  Copying first conversion files...")
					if exportModel.modelType == "Static" or 
					   exportModel.modelType == "Animated" and (exportType.selected != "Current Scene" or currentComponent == undefined ) then 
					(
					    -- base model files
						for ext in #(".mesh", ".material", ".physics") do (
							local src = firstExportDir + exportModel.baseName + ext,
							      dst = exportDirectory + exportModel.baseName + ext
							if doesFileExist src then (
								log ("    " + src + " to " + dst)
								deleteFile dst
								copyFile src dst
							)
						)
					
					)
					
					-- skeleton
					if exportModel.modelType == "Animated" then (
							local src = firstExportDir + exportModel.baseName + ".skeleton",
							      dst = exportDirectory + exportModel.baseName + ".skeleton"
							if doesFileExist src then (
								log ("    " + src + " to " + dst)
								deleteFile dst
								copyFile src dst
							)
					)
				)
			)
		) 
		
/* ---------  repository import specs and .asset file templates ------------ */

		local importSpec =  #(#(".mesh", "Meshes"),
						      #(".material", "Materials"),
							  #(".physics", "Physics"),
							  #(".skeleton", "Skeletons"),
							  #(undefined, "AssetDefinitions"),
							  #(undefined, "Fonts"),
							  #(undefined, "GpuPrograms"),
							  #(undefined, "Icons"),
							  #(undefined, "Imagefiles"),
							  #(undefined, "Interface"),
							  #(undefined, "Interface\\FrameXML"),
							  #(undefined, "Interface\\Imagesets"),
							  #(undefined, "Misc"),
							  #(undefined, "Particles"),
							  #(undefined, "Scripts"),
							  #(undefined, "Sounds"),
							  #(undefined, "SpeedTree"))
							  
		local meshAssetDefTemplate = 
"<?xml version=\"1.0\" encoding=\"utf-8\"?>
<AssetDefinition>
  <Name>%_Mesh</Name>
  <Description>%</Description>
  <Category>%</Category>
  <Type>Mesh</Type>
  <Files>
    <AssetFile>
      <TargetFile>Meshes%%.mesh</TargetFile>
      <FileType>Mesh</FileType>
    </AssetFile>
    <AssetFile>
      <TargetFile>AssetDefinitions%%_Material.asset</TargetFile>
      <FileType>AssetDefinition</FileType>
    </AssetFile>
    <AssetFile>
      <TargetFile>Physics%%.physics</TargetFile>
      <FileType>Physics</FileType>
    </AssetFile>%
  </Files>
</AssetDefinition>
" 

		local skelAssetTemplate =
"		
    <AssetFile>
      <TargetFile>Skeletons%%.skeleton</TargetFile>
      <FileType>Skeleton</FileType>
    </AssetFile>
"

		local materialAssetDefTemplate = 
"<?xml version=\"1.0\" encoding=\"utf-8\"?>
<AssetDefinition>
  <Name>%_Material</Name>
  <Description>%</Description>
  <Type>Material</Type>
  <Files>
    <AssetFile>
      <TargetFile>Materials%%.material</TargetFile>
      <FileType>Material</FileType>
    </AssetFile>
	%
  </Files>
</AssetDefinition>
"

		local textureAssetDefTemplate = "
    <AssetFile>
      <TargetFile>Textures%%</TargetFile>
      <FileType>Texture</FileType>
    </AssetFile>"
						
											
		-- repository import of exportModel
		fn repositoryImport exportDirectory rootDirectory = 
		(
			if enableRepositoryImport.checked then (
				-- copy files from export to appropriate asset directories
				log ("Repository import " + rootDirectory)
				showStatus "importing asset files."
				if rootDirectory[rootDirectory.count] != "\\" then rootDirectory += "\\" 
				for spec in importSpec do (
					local dir = rootDirectory + spec[2] + "\\"
					makeDeepDir dir
					if spec[1] != undefined and (spec[1] != ".material" or not preserveDotMaterial.checked) then (
						local src = exportDirectory + exportModel.baseName + spec[1],
					          dst = dir + exportModel.baseName + spec[1]
						if doesFileExist src then (
							log ("  importing " + spec[1] + ": " + dst + " from " + src)
							deleteFile dst
							copyFile src dst
						)
					)
				)
				
				if enableImportTextures.enabled and enableImportTextures.checked then (
					-- copy across any discoverd texture files
					local texDir = rootDirectory + "Textures"
					makeDeepDir texDir
					log "  importing discovered texture files..."
					for t in exportModel.textures do (
						local dst = texDir + "\\" + (getFileNameFile t) + (getFileNameType t)
						if t != dst then (
							deleteFile dst
							copyFile t dst
							log ("    " + dst + " from " + t)
						)
						else
							log ("    " + t + " already in repository")
					)
					if enableSaveTextures.enabled and enableSaveTextures.checked then (
						-- save copies in export dir
						local texDir = exportDirectory + "Textures"
						makeDeepDir texDir
						log "  saving discovered texture files..."
						for t in exportModel.textures do (
							local dst = texDir + "\\" + (getFileNameFile t) + (getFileNameType t)
							if t != dst then (
								deleteFile dst
								copyFile t dst
								log ("    " + dst + " from " + t)
							)
							else
								log ("    " + t + " already saved")
						)
					)
				)
				
				-- build asset definition files, first mesh
				local cm = exportModel,
					  assetDir = rootDirectory + "AssetDefinitions",
				      assetFile = assetDir + "\\" + cm.baseName + "_Mesh.asset"
				log ("  building mesh asset definition " + assetFile)
				makeDeepDir assetDir
				deleteFile assetFile
				local af = createFile assetFile,
				      skeldef = StringStream ""
				if cm.modelType == "Animated" then format skelAssetTemplate "\\" cm.baseName to:skeldef 
				format meshAssetDefTemplate cm.baseName cm.assetDescription cm.assetCategory \
				                            "\\" cm.baseName "\\" cm.baseName "\\" cm.baseName (skeldef as string) to:af
				close af
				deleteFile (exportDirectory + cm.baseName + "_Mesh.asset")
				copyFile assetFile (exportDirectory + cm.baseName + "_Mesh.asset") -- leave copy in export dir for backup
				
				-- then materials
				assetFile = assetDir + "\\" + cm.baseName + "_Material.asset"
				log ("  building material asset definition " + assetFile)
				deleteFile assetFile
				-- textures
				local texDefs = StringStream ""
				for t in exportModel.textures do (
					format textureAssetDefTemplate "\\" ((getFileNameFile t) + (getFileNameType t)) to:texDefs
					log ("    including texture " + ((getFileNameFile t) + (getFileNameType t)))
				)
				local af = createFile assetFile
				format materialAssetDefTemplate cm.baseName cm.assetDescription "\\" cm.baseName (texDefs as string) to:af
				close af
				deleteFile (exportDirectory + cm.baseName + "_Material.asset")
				copyFile assetFile (exportDirectory + cm.baseName + "_Material.asset") -- leave copy in export dir for backup
			)
		)
		
		-- open exportModel in viewer
		fn openInViewer rootDirectory = 
		(
			if enableOpenInViewer.checked then (
				local cmd = "start /d\"" + viewerPath + "\" " + viewerExe + " ",
				      meshFile = " \"" + rootDirectory + "\\Meshes\\" + exportModel.baseName + ".mesh\""
--print (cmd + "--mesh_file " + meshFile + " --repository_path " + rootDirectory)					  
				DOSCommand (cmd + "--mesh_file " + meshFile + " --repository_path " + rootDirectory)
			)
		)
		
		-- pick next export directory 
		fn pickExportDir rootDirectory baseName lastUsed:false =
		(
			local exportsINI = rootDirectory + "\\Exports\\" + baseName + "\\exports.ini"
			makeDeepDir (rootDirectory + "\\Exports\\" + baseName)
			if lastUsed then (
				-- retrieve last used directory from export ini
				return getINISetting exportsINI "Directories" "lastExportTo"
			)
			else (
				local ed =
					if enableSequenceFolders.checked then (
						local erd = rootDirectory + "\\Exports\\" + baseName + "\\autoexport_",
			                  dirs = getDirectories (erd + "*"),
					    	  nextNo = if dirs.count == 0 then -1 else (
							      local n = -1, i = dirs.count
							      sort dirs
								  while i > 0 and n < 0 do 
								      try n = (substring dirs[i] (dirs[i].count - 3) 3) as integer catch ()
								  n
							  )
						nextNo += 1
						erd + to000String nextNo + "\\"
					)
					else
						rootDirectory + "\\Exports\\" + baseName + "\\autoexport\\"
				-- save selected export directory in ini file (used by LOD merger, among others)
				setINISetting exportsINI "Directories" "lastExportTo" ed 
				return ed
			)
		)
		
		-- pre-export LOD validate & prep (too much to do in setUIState processing)
		fn prepLODProcessing = 
		(
			if enableLODProcessing.enabled and enableLODProcessing.checked do (
				-- extract LOD sets
				selectedLODSets =
					for e in modelSet where e.lodStatus == "Base Mesh" collect (
						local baseName = getINISetting e.modelDefFile "Model" "baseName"
						#(e) + (for f in modelSet where f.lodStatus == "LOD Level" and f.lodBaseMesh == baseName collect f)
					)
				-- cull those not selected in some way for export
				if exportType.selected == "Checked Models" then 
					selectedLODSets = for ls in selectedLODSets where
						(for e in ls where e.checked collect e).count > 0 collect ls
				else if exportType.selected == "Current Scene" or exportType.selected == "Current Model" then (
					selectedLODSets = for ls in selectedLODSets where
						(for e in ls where getINISetting e.modelDefFile "Model" "baseName" == m.baseName collect r).count > 0 collect ls
					if selectedLODSets.count == 0 then (
						messageBox "Merge Manual LOD Meshes enabled but current Model Def has no LOD details in current Model Set, Export aborted." beep:true
						return false
					)
				)
				-- validate LOD details
				for ls in selectedLODSets do (
					for e in ls where e.lodStatus == "LOD Level" do (
						if e.lodDistance == 0 then (
							messageBox "Merge Manual LOD Meshes enabled but a LOD Level model has LOD Distance of 0, Export aborted." beep:true
							return false
							)
						for f in ls where e.lodStatus == "LOD Level" do if e != f and e.lodDistance == f.lodDistance do (
							messageBox "Merge Manual LOD Meshes enabled but two or more LOD Levels have equal LOD Distances, Export aborted." beep:true
							return false
						)
					)
				)
			)
			return true
		)
		
		fn doLODMerges = if enableLODProcessing.enabled and enableLODProcessing.checked do
		(
			showStatus "Performing LOD merges."
			-- extract LOD sets and cull those not selected in some way for export
			local lodSets =
				for e in modelSet where e.lodStatus == "Base Mesh" collect (
					local baseName = getINISetting e.modelDefFile "Model" "baseName"
					#(e) + (for f in modelSet where f.lodStatus == "LOD Level" and f.lodBaseMesh == baseName collect f)
				)
			if exportType.selected == "Checked Models" then 
				lodSets = for i in 1 to lodSets.count where
					(for f in lodSets[i] where f.checked collect f).count > 0 collect lodSets[i]
					
			-- process LOD sets selected in prepLODProcessing()
			for s in selectedLODSets do (
				-- gather model defs
				local saveM = m,
					  setModels = for e in s collect (
					      m = Model()
						  if loadModel e.modelDefFile loadIntoUI:false then m  -- side-effects model in m
						  else continue
					  )
				m = saveM

				-- do the merge (for now in repositories of LOD base mesh, assumes LOD Level meshes in same repos)
				for r in setModels[1].repositories do (
					if repositoryChoice.selected == "All" or 				   		-- all 
					   repositoryChoice.selected == "Checked" and r.checked then    -- checked 
					(
						local exportDirectory = pickExportDir r.rootDirectory setModels[1].baseName lastUsed:true,
							  args, baseMeshName
						if exportDirectory == "" or not doesFileExist exportDirectory then
							throw ("Unabled to perform LOD merge, missing Exports directory for " + setModels[1].baseName)
						local lodExportDir = exportDirectory + "LOD\\",
						      meshDir = r.rootDirectory + "\\Meshes\\"
						makeDeepDir lodExportDir
						-- the merge is done using the meshes from the repos Mesh directory, first copied into LOD exports subdir
						-- set up conversionTool args
						log ("Processing LOD merge for " + setModels[1].baseName + " in repository " + meshDir)
						for j in 1 to setModels.count do (
							local meshName = setModels[j].baseName + ".mesh"
							if not doesFileExist (meshDir + meshName) then
								throw ("Manual LOD component mesh file missing: " + (meshDir + meshName))
							if j == 1 then (
								args = " " + meshName
								baseMeshName = meshName
							)
							else (
								args = " --manual_lod " + (s[j].lodDistance * 1000) as string + " " + meshName + args
								log ("  including LOD mesh " + meshName + " at " + s[j].lodDistance as string + " meters")
							)
							-- put LOD component mesh copies in LOD export subdir
							local src = meshDir + meshName,
						          dst = lodExportDir + meshName
							deleteFile dst
							copyFile src dst
						)
						-- run conversion merge into base mesh
						runConversionTool args "" lodExportDir
						-- copy merged base mesh back to repository
						deleteFile (meshDir + baseMeshName)
						copyFile (lodExportDir + baseMeshName) (meshDir + baseMeshName)
					)
				)
			)
		)
															
		-- handle export
		on export pressed do 
		(
			if not prepLODProcessing() then return 0
			local currentScene = maxFilePath + maxFileName
			logNo = 0 -- reset conversion tool log number
			openLog()
			
			-- turnoff CV capsule edit mode if on
			if mro != undefined and mro.editCapsule.checked then (
				mro.editCapsule.checked = false
				mro.editCapsule.changed false
			)
		
			-- do current model or seleced models in model set
			local models
			if exportType.selected == "All Models" or exportType.selected == "Checked Models" then (
				local saveM = m
				models = for e in modelSet where exportType.selected == "All Models" or e.checked collect (
					m = Model()
					log ("Loading model def " + e.modelDefFile)
					if loadModel e.modelDefFile loadIntoUI:false then m  -- side-effects model in m
					else continue
				)
				m = saveM
			)
			else
				models = #(m)
			
			try 
			(
				-- loop over model files
				for em in models do (
					local firstExportDir = undefined
					log ("Exporting model " + em.baseName)
					exportModel = em

					-- loop over destination repositories
					for r in exportModel.repositories do (
						if repositoryChoice.selected == "All" or 				   		-- all 
						   repositoryChoice.selected == "Checked" and r.checked then    -- checked 
						(
							local exportDirectory = pickExportDir r.rootDirectory exportModel.baseName
							log ("Destination repository " + r.rootDirectory)
							log ("  Export directory " + exportDirectory)
							
							-- ensure exports directory exists
							makeDeepDir exportDirectory

							-- ** note that currentComponent has been set by setUIStates based on export options
							
						 	-- switch on export type
							if exportType.selected == "Current Scene" then ( 
								-- do export steps
								colladaExport currentScene exportDirectory firstExportDir
								modelConversion exportDirectory firstExportDir
								repositoryImport exportDirectory r.rootDirectory
								openInViewer r.rootDirectory
							)
							else  (   -- current model or model set
								if exportModel.modelType == "Static" then (
									colladaExport exportModel.baseScene exportDirectory firstExportDir
									modelConversion exportDirectory firstExportDir
									repositoryImport exportDirectory r.rootDirectory
									openInViewer r.rootDirectory
								)
								else if exportModel.modelType == "Animated" then (
									if currentComponent != undefined then (
										-- export mode == Current Animation, just do DAE export for this component
										local scene = if exportModel.encoding == "Scene Files" then currentComponent.fileName else exportModel.baseScene 
										colladaExport scene exportDirectory firstExportDir
									)
									else (
										-- model, collada export of base scene and selected components
										colladaExport exportModel.baseScene exportDirectory firstExportDir
										for c in exportModel.components where componentChoice == "All" or c.checked do (
											local scene 
											if exportModel.encoding == "Scene Files" then
												scene = c.fileName
											else (
												if c.animName == "base" then continue  -- skip base component, already done in base scene export
												scene = exportModel.baseScene
											)
											currentComponent = c
											colladaExport scene exportDirectory firstExportDir
										)
										currentComponent = undefined -- restore component select status
									)
									-- then rest of export steps
									modelConversion exportDirectory firstExportDir 
									repositoryImport exportDirectory r.rootDirectory
									openInViewer r.rootDirectory
								)
							)
							-- record first repository, subsequent export-step calls will copy from it as needed
							if firstExportDir == undefined then firstExportDir = exportDirectory 
						)
					)
					log "Model Export Complete."
				)
				
				-- do any LOD merges after all models converted
				doLODMerges()
				
				showStatus "Export Complete."
				log "Export Complete.\n"
				doscommand "echo •"
			)
			catch (
				messageBox (getCurrentException()) beep:true
				showStatus "Export Failed."
				log ("** Export Failed: " + getCurrentException())
			)
			
			-- clean-up, restore max scene file if needed
			if maxFilePath + maxFileName != currentScene then
				loadMaxFile currentScene useFileUnits:true
				
			closeLog()
		)
		
		on MVE_Export open do setUIStates()
		
		on exportType selected i do setUIStates() 	
		on componentChoice selected i do setUIStates() 	
		on enableAddPhysics changed val do (setCVCapsuleUIState(); setUIStates())
		on checkSkinBoneCountBtn changed val do checkSkinBoneCount()
		on showLogBtn pressed do showLog()
		on clearLogBtn pressed do clearLog()
		
	)
	
	-- tool configuration
	rollout MVE_Config "Configuration"
	(
		group "Multiverse Tool paths"
		(
			label L0 "Model Converter:" align:#left
			button pickModelConverter "Choose..." width:250
			label L1 "Model Viewer:" align:#left
			button pickModelViewer "Choose..." width:250
			label L2 "World Editor:" align:#left
			button pickWorldEditor "Choose..." width:250
			label L3 "Asset Importer:" align:#left
			button pickAssetImporter "Choose..." width:250
		)
		spinner setToolTimeout "Converter Timeout (secs):" type:#integer range:[5,100,30] align:#left offset:[0,6]
		label L4 "Log File :" align:#left
		button pickLogFile "Choose..." width:250
		button reset "Reset to defaults" align:#right offset:[-4,6]
		
		fn loadConfig =
		(
			-- load current or create new multivers config file
			configFile = (getDir #plugcfg) + "\\" + MVE_CONFIG_FILE
			modelConverter = getINISetting configFile "Paths" "modelConverter" 
			modelViewer = getINISetting configFile "Paths" "modelViewer" 
			worldEditor = getINISetting configFile "Paths" "worldEditor"
			assetImporter = getINISetting configFile "Paths" "assetImporter"
			toolTimeout = getINISetting configFile "Timeouts" "modelConverter" 
			logFileName = getINISetting configFile "Paths" "logFileName"
			if modelConverter == "" then (
				modelConverter = "C:\\Program Files\\Multiverse Tools\\Multiverse Model Viewer\\ConversionTool.exe"
				setINISetting configFile "Paths" "modelConverter" modelConverter
			)
			converterPath = getFilenamePath modelConverter
			converterExe = getFilenameFile modelConverter
			if modelViewer == "" then (
				modelViewer = "C:\\Program Files\\Multiverse Tools\\Multiverse Model Viewer\\ModelViewer.exe"
				setINISetting configFile "Paths" "modelViewer" modelViewer 
			)
			viewerPath = getFilenamePath modelViewer 
			viewerExe = getFilenameFile modelViewer 
			if worldEditor == "" then (
				worldEditor = "C:\\Program Files\\Multiverse Tools\\Multiverse World Editor\\WorldEditor.exe"
				setINISetting configFile "Paths" "worldEditor" worldEditor
			)
			if assetImporter == "" then (
				assetImporter = "C:\\Program Files\\Multiverse Tools\\AssetTools\\AssetImporter.exe"
				setINISetting configFile "Paths" "assetImporter" assetImporter 
			)
			if toolTimeout == "" then toolTimeout = 30
			toolTimeout = try toolTimeout as integer catch 30
			if logFileName == "" then (
				logFileName = sysinfo.tempdir + "multiverse_export.log"
				setINISetting configFile "Paths" "logFileName" logFileName 
			)
			-- load up UI
			pickModelConverter.text = fitButtonText modelConverter 250; pickModelConverter.toolTip = modelConverter 
			pickModelViewer.text = fitButtonText modelViewer 250; pickModelViewer.toolTip = modelViewer 
			pickWorldEditor.text = fitButtonText worldEditor 250; pickWorldEditor.toolTip = worldEditor
			pickAssetImporter.text = fitButtonText assetImporter 250; pickAssetImporter.toolTip = assetImporter 
			setToolTimeout.value = toolTimeout
			pickLogFile.text = fitButtonText logFileName 250; pickLogFile.toolTip = logFileName 
		)
		
		on MVE_Config open do loadConfig()
		
		-- handle presses on tool path buttons, update UI & .ini file
		on pickModelConverter pressed do
		(
			local f = getOpenFilename caption:"Model Conversion Tool executable" filename:"ConversionTool.exe" types:"Exe(*.exe)|*.exe"
			if f != undefined then (
				modelConverter = f
				setINISetting configFile "Paths" "modelConverter" modelConverter
				pickModelConverter.text = fitButtonText modelConverter 250; pickModelConverter.toolTip = modelConverter
				converterPath = getFilenamePath modelConverter
				converterExe = getFilenameFile modelConverter
			)
		)
		
		on pickModelViewer pressed do
		(
			local f = getOpenFilename caption:"Model Viewer executable" filename:"ModelViewer.exe" types:"Exe(*.exe)|*.exe"
			if f != undefined then (
				modelViewer = f
				setINISetting configFile "Paths" "modelViewer" modelViewer
				pickModelViewer.text = fitButtonText modelViewer 250; pickModelViewer.toolTip = modelViewer 
			)
		)
		
		on pickWorldEditor pressed do
		(
			local f = getOpenFilename caption:"World Editor Tool executable" filename:"WorldEditor.exe" types:"Exe(*.exe)|*.exe"
			if f != undefined then (
				worldEditor = f
				setINISetting configFile "Paths" "worldEditor" worldEditor 
				pickWorldEditor.text = fitButtonText worldEditor 250; pickWorldEditor.toolTip = worldEditor 
			)
		)
		
		on pickAssetImporter pressed do
		(
			local f = getOpenFilename caption:"Asset Importer Tool executable" filename:"AssetImporter.exe" types:"Exe(*.exe)|*.exe"
			if f != undefined then (
				assetImporter = f
				setINISetting configFile "Paths" "assetImporter" assetImporter 
				pickAssetImporter.text = fitButtonText assetImporter 250; pickAssetImporter.toolTip = assetImporter 
			)
		)
		
		on setToolTimeout changed val do 
		(
			toolTimeout = val
			setINISetting configFile "Timeouts" "modelConverter" (val as string)
		) 
			
		on pickLogFile pressed do
		(
			local f = getSaveFilename caption:"Export Log File" types:"Log(*.log)|*.log"
			if f != undefined then (
				logFileName = f
				setINISetting configFile "Paths" "logFileName" logFileName 
				pickLogFile.text = fitButtonText logFileName 250; pickLogFile.toolTip = logFileName 
			)
		)
		
		on reset pressed do if queryBox "Confirm configuration reset to defaults" beep:true do
		(
			deleteFile configFile
			loadConfig()
		)
	)
			
	-- macro execute, open export tool floater
	
	on execute do
	(
		-- (re)open the export tool window
		if MVE_toolWindow != undefined then closeRolloutFloater MVE_toolWindow
		MVE_toolWindow = newRolloutFloater ("Multiverse Export Tool v" + TOOL_VERSION as string) 300 764
		addRollout MVE_ModelSet MVE_toolWindow rolledUp:true
		addRollout MVE_Model MVE_toolWindow rolledUp:false
		addRollout MVE_Repositories MVE_toolWindow rolledUp:true
		addRollout MVE_Export MVE_toolWindow rolledUp:true
		addRollout MVE_Config MVE_toolWindow rolledUp:true
		-- load last model set & model
		MVE_ModelSet.loadLastDef()
		MVE_Model.loadLastDef()
	)
)

/*
macroScript MultiverseValidateMacro
	category:"Multiverse Tools" 
	tooltip:"Opens the Multiverse Model Validator Dialog" 
	buttonText:"Validate Model Tool"
(
	on execute do print "Hello from MultiverseValidateMacro!"
)
*/

macroScript MultiverseOpenAssetImporterMacro
	category:"Multiverse Tools" 
	tooltip:"Opens the Multiverse Asset Importer Application" 
	buttonText:"Open Asset Importer"
(
	MVE_CONFIG_FILE = "multiverse-tools.ini"
	on execute do 
	(
		local configFile = (getDir #plugcfg) + "\\" + MVE_CONFIG_FILE,
		      assetImporter = getINISetting configFile "Paths" "assetImporter" 
		if assetImporter == "" then (
			assetImporter = "C:\\Program Files\\Multiverse Tools\\AssetTools\\AssetImporter.exe"
			setINISetting configFile "Paths" "assetImporter" assetImporter 
		)
	
		ShellLaunch assetImporter ""
	)
)

macroScript MultiverseOpenModelViewerMacro
	category:"Multiverse Tools" 
	tooltip:"Opens the Multiverse Model Viewer Application" 
	buttonText:"Open Model Viewer"
(
	MVE_CONFIG_FILE = "multiverse-tools.ini"
	on execute do 
	(
		local configFile = (getDir #plugcfg) + "\\" + MVE_CONFIG_FILE,
		      modelViewer = getINISetting configFile "Paths" "modelViewer" 
		if modelViewer == "" then (
			modelViewer = "C:\\Program Files\\Multiverse Tools\\Multiverse Model Viewer\\ModelViewer.exe"
			setINISetting configFile "Paths" "modelViewer" modelViewer 
		)
		ShellLaunch modelViewer ""
	)
)

-- register Multiverse menu
menuContextID = 0x4affe924
if menuMan.registerMenuContext menuContextID then
(
    -- remove it if already there
	local mum = menuMan.findMenu("Multiverse")
	if mum != undefined then (
		mm = menuMan.getMainMenuBar()
		for i in 1 to mm.numItems() do
			if (mm.getItem i).getTitle() == "Multiverse" then (
				mm.removeItemByPosition i
				menuMan.unregisterMenu mum
				exit
			)
		)
				
	-- Create the multiverse menu
	local subMenu = menuMan.createMenu "Multiverse",
	      exportItem = menuMan.createActionItem "MultiverseExportMacro" "Multiverse Tools",
		  validateItem = menuMan.createActionItem "MultiverseValidateMacro" "Multiverse Tools",
	      viewerItem = menuMan.createActionItem "MultiverseOpenModelViewerMacro" "Multiverse Tools",
	      importerItem = menuMan.createActionItem "MultiverseOpenAssetImporterMacro" "Multiverse Tools"
	exportItem.setTitle "Export Tool..."; exportItem.setUseCustomTitle true
	if validateItem != undefined then (
		validateItem .setTitle "Validate Model Tool..."; validateItem .setUseCustomTitle true
	)
	viewerItem .setTitle "Open Model Viewer..."; viewerItem .setUseCustomTitle true
	importerItem .setTitle "Open Asset Importer..."; importerItem .setUseCustomTitle true
	subMenu.addItem exportItem -1
	-- subMenu.addItem validateItem -1
	subMenu.addItem (menuMan.createSeparatorItem()) -1
	subMenu.addItem viewerItem -1
	subMenu.addItem importerItem -1
	
	-- add it to the main menu bar
	local subMenuItem = menuMan.createSubMenuItem "Multiverse" subMenu,
	      mainMenuBar = menuMan.getMainMenuBar()
	mainMenuBar.addItem subMenuItem (mainMenuBar.numItems())
	menuMan.updateMenuBar()
)

-- for testing
macros.run "Multiverse Tools" "MultiverseExportMacro"


 
