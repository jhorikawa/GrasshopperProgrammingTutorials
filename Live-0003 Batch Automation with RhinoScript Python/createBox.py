import rhinoscriptsyntax as rs
import Rhino.Geometry as geo
import math

def Run(xNum, yNum):
    if xNum is None: 
        rs.MessageBox("No number provided for x")
        return
    if yNum is None:
        rs.MessageBox("No number provided for y")
        return
    
    rs.EnableRedraw(False)
    boxes = []
    for i in range(xNum):
        for n in range(yNum):
            points = []
            for s in range(2):
                for t in range(4):
                    x = math.cos(math.radians(45 + 90 * t)) * math.sqrt(2)
                    y = math.sin(math.radians(45 + 90 * t)) * math.sqrt(2)
                    z = -1 + 2 * s
                    point = geo.Point3d(x, y, z)
                    points.append(point)
            box = rs.AddBox(points)
            
            box = rs.RotateObject(box, geo.Point3d(0,0,0), 90 / (xNum - 1) * i, geo.Vector3d.ZAxis)
            box = rs.RotateObject(box, geo.Point3d(0,0,0), 90 / (yNum - 1) * n, geo.Vector3d.XAxis)
            
            box = rs.MoveObject(box, geo.Vector3d(4 * i, 4 * n, 0))
            boxes.append(box)
    rs.EnableRedraw(True)
    
    return boxes
    
#Run()    