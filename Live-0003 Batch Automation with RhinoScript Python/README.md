# Batch Automation with RhinoScript Python

## Requirements
- Rhinoceros 6

## Reference pages
- RhinoScriptSyntax: https://developer.rhino3d.com/api/RhinoScriptSyntax/
- Find files in directory: https://stackoverflow.com/questions/3964681/find-all-files-in-a-directory-with-extension-txt-in-python

## Audience target
- Beginner in RhinoScript and want to learn the basics of batch automation.

## What you will learn
- Basics of RhinoScript Python.
- Reuse function saved in external python file.
- How to batch import files.
- How to batch export files.

## Step 1: Define function to create bunch of rotated boxes.

```python
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
```

## Step 2: Define function to export objects to files.

```python
import rhinoscriptsyntax as rs

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
```

## Step 3: Define function to import files.

```python
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
```

## Step 4: Create boxes and export them as files.

```python
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

```

## Step 5: Import exported files and export them again in different file format.

```python
import rhinoscriptsyntax as rs
import importObjs
import exportObjs

dir = rs.BrowseForFolder(None, "Select folder which contains objects you want to import")
objs = importObjs.Run(dir, "3dm")
exportObjs.Run(objs, dir, "obj")
rs.DeleteObjects(objs)

```