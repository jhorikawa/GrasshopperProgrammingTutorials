import rhinoscriptsyntax as rs
import importObjs
import exportObjs

dir = rs.BrowseForFolder(None, "Select folder which contains objects you want to import")
objs = importObjs.Run(dir, "3dm")
exportObjs.Run(objs, dir, "obj")
rs.DeleteObjects(objs)
