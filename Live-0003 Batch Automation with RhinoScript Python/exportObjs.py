import rhinoscriptsyntax as rs
#import createBox

def Run(objs, dir, ext):
    rs.EnableRedraw(False)
    rs.UnselectAllObjects()
    for i in range(len(objs)):
        obj = objs[i]
        filepath = dir + "/exported" + str(i) + "." + ext
        rs.SelectObject(obj)
        rs.Command("!_-Export \"" + filepath + "\" -Enter -Enter")
        rs.UnselectAllObjects()
    rs.EnableRedraw(True)    

#xNum = rs.GetInteger("X number", 5, 1)
#yNum = rs.GetInteger("Y number", 5, 1)
#boxes = createBox.Run(xNum, yNum)
#if boxes is None or len(boxes) == 0:
#    rs.MessageBox("No objects to export")
#else:
#    dir = rs.BrowseForFolder(None, "Select a folder to save files")
#    Run(boxes, dir, "3dm")

