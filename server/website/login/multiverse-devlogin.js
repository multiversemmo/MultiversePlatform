// Check to see if the select input contains an option with the given value
function ContainsOption(elem, val) {
  for (var i = 0; i < elem.options.length; ++i) {
    if (elem.options.value == val) {
      return true;
    }
  }
  return false;
}
// Add any worlds that we want to show up (from the login settings)
function AddLocalWorlds() {
  if (!window.external) {
    return;
  }
  var world_options = new Array();
  var len = window.external.GetWorldCount();
  var preferredId = window.external.GetPreferredWorld();
  for (var i = 0; i < len; ++i) {
    var entry = window.external.GetWorldEntry(i);
    world_options[world_options.length] = entry;
  }
  elem = document.getElementById("world_id");
  if (elem && elem.options) {
    for (var i = 0; i < world_options.length; ++i) {
      if (!ContainsOption(elem, world_options[i].worldId)) {
        var option = new Option();
        option.value = world_options[i].worldId;
        option.appendChild(document.createTextNode(world_options[i].worldName));
        if (preferredId == "" && world_options[i].isDefault) {
          option.selected = true;
        } else if (preferredId == world_options[i].worldId) {
          option.selected = true;
        }
        elem.appendChild(option);
      }
    }
    if (preferredId != "" && !ContainsOption(elem, preferredId)) {
      var option = new Option();
      option.value = preferredId;
      option.appendChild(document.createTextNode('Custom World'));
      option.selected = true;
      elem.appendChild(option);
    }
  } else {
    //alert("invalid element: world_id");
  }
}

function HandleLoad() {
  var account_elem = document.getElementById('account');
  var password_elem = document.getElementById('password');
  if (account_elem && password_elem) {
    if (!account_elem.value || account_elem.value.length == 0) {
      account_elem.focus();
    } else {
      password_elem.focus();
    }
  }
  AddLocalWorlds();
  UpdateWidgetDisplay();
}

function UpdateWidgetDisplay() {
  if (window.external && window.external.Username != "") {
    var txt_elem = document.getElementById("account");
    txt_elem.value = window.external.Username;
  }

  if (window.external && window.external.Password != "") {
    var pass_elem = document.getElementById("password");
    pass_elem.value = window.external.Password;
  }

  if (window.external && window.external.RememberUsername) {
    var checkbox_elem = document.getElementById("remember_username");
    checkbox_elem.checked = true;
  }

  var has_error = false;
  var has_status = false;

  if (window.external && window.external.ErrorMessage != "") {
    has_error = true;
    var txt_elem = document.getElementById("error");
    txt_elem.innerText = window.external.ErrorMessage;
  } else {
    has_error = false;
  }

  if (window.external && window.external.StatusMessage != "") {
    has_status = true;
    var txt_elem = document.getElementById("status");
    txt_elem.innerText = window.external.StatusMessage;
  } else {
    has_status = false;
  }

  var elem = document.getElementById("world_id");
  if (elem) {
    if (elem.options && elem.options.length > 1) {
      has_world_list = true;
    } else {
      has_world_list = false;
    }
  } else {
    //alert("invalid element: world_id");
  }

  // alert('update widget display: ' + has_status + ', ' + has_error);

  // Should we display the status widget?  
  var error_status_table = document.getElementById("status_error_table");
  var pop_section = document.getElementById("pop_section");
  
  if (has_error || has_status) {
    // Turn off news section to make room 
    if (pop_section != null) {
      pop_section.style.display = 'none';
    }
    
    // Show the error/status table
    error_status_table.style.display = 'block';
    var error_section = document.getElementById("error_section");
    var status_section = document.getElementById("status_section");
    if (has_error) {
      error_section.style.display = 'block';
    } else {
      error_section.style.display = 'none';
    }
    if (has_status) {
      status_section.style.display = 'block';
    } else {
      status_section.style.display = 'none';
    }
  } else {
    error_status_table.style.display = 'none';
    if (pop_section != null) {
      
      pop_section.style.display = 'inline';
    }
  }
  
  // Should we display the world selection widget?
  var world_section = document.getElementById("world_section");
  if (world_section != null) {
    if (has_world_list) {
      world_section.style.display = 'block';
    } else {
      world_section.style.display = 'none';
    }
  }
}

function Login() {
  var elem;
  elem = document.getElementById("account");
  if (elem) {
    if (elem.value == "") {
      elem.focus();
      return;
    } else {
      window.external.Username = elem.value;
    }
  }
  elem = document.getElementById("password");
  if (elem) {
    if (elem.value == "") {
      elem.focus();
      return;
    } else {
      window.external.Password = elem.value;
    }
  }
  elem = document.getElementById("remember_username");
  if (elem) {
    if (elem.checked) {
      window.external.RememberUsername = true;
    } else {
      window.external.RememberUsername = false;
    }
  }
  elem = document.getElementById("full_scan");
  if (elem) {
    if (elem.checked) {
      window.external.FullScan = true;
    } else {
      window.external.FullScan = false;
    }
  }
  elem = document.getElementById("world_id");
  if (elem) {
    window.external.LoginMaster(elem.value);
  } else {
    return;
  }
  //UpdateWidgetDisplay();
}

// Set world selection based on clicking icon
//function setSelection(txt) {
//  var selObj = document.getElementById("world_id");
//  for (i=0; i<selObj.options.length; i++) {
//    if (selObj.options[i].value==txt) {
//      selObj.selectedIndex = i;
//    }
//  }
//}
