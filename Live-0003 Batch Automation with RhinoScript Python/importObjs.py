import rhinoscriptsyntax as rs
import os

def Run(dir, ext):
    if dir is None:
        rs.MessageBox("No directory path defied")
        return
    
    files = []
    for file in os.listdir(dir):
        if file.endswith("." + ext):
            files.append(dir + "/" + file)
    
    rs.EnableRedraw(False)
    rs.UnselectAllObjects()
    importedObjs = []
    for filepath in files:
        rs.Command("!_-Import \"" + filepath + "\" -Enter -Enter")
        selectedObjs = rs.SelectedObjects()
        importedObjs.extend(selectedObjs)
    rs.UnselectAllObjects()       
    rs.EnableRedraw(True)
    
    return importedObjs


#dir = rs.BrowseForFolder(None, "Select folder which contains objects you want to import")
#Run(dir, "3dm")
