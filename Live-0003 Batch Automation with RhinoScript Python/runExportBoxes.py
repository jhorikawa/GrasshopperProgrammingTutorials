import rhinoscriptsyntax as rs
import createBox
import exportObjs

xNum = rs.GetInteger("X number", 5, 1)
yNum = rs.GetInteger("Y number", 5, 1)
boxes = createBox.Run(xNum, yNum)
if boxes is None or len(boxes) == 0:
    rs.MessageBox("No objects to export")
else:
    dir = rs.BrowseForFolder(None, "Select a folder to save files")
    exportObjs.Run(boxes, dir, "3dm")
