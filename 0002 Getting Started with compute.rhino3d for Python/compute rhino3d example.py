import rhino3dm
import random
import compute_rhino3d
import compute_rhino3d.Util
import compute_rhino3d.Curve
import compute_rhino3d.Mesh

compute_rhino3d.Util.authToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwIjoiUEtDUyM3IiwiYyI6IkFFU18yNTZfQ0JDIiwiYjY0aXYiOiJVQmZNMFhtbTdOQjk3QU0zZEo3VFdnPT0iLCJiNjRjdCI6IndqU0I1QzRXT29BRlhvTmtGVTRrTHhIeDQ1Nm9TUjdwQ09aNVpJN1UzSjV3OWJ6U0ZhcjcvUmJmMGNjT2E2M2ZZamt3TzZ5QzZCVXk3Y1lDQ2NoT3VQT3JuQzBNWnBmcUNRTHIybFEwb1FGTWM1ZWJYZnlOU3ViTnNJc1hUVFViN1pFMjJEcUZQVzlxSmdFOFlqRkRzc2J3T0tRYy9GdlJsZFlJeVNIQi9UV1BsYzR4OFF5QVBCQjI4MUdYbU92endNbHA2bFpkbVp5MzREMlRsQ1gxc0E9PSIsImlhdCI6MTU1NjYzODYzMX0.nEBRA_ho1xBommg0TWzx39sf7C1_oXZnBtBgrePrgUA"
#compute_rhino3d.Util.url = "http://127.0.0.1:8081/"

model = rhino3dm.File3dm()
curves = []
for i in range(20):
    pt = rhino3dm.Point3d(random.uniform(-10, 10), random.uniform(-10, 10), 0)
    #model.Objects.AddPoint(pt)
    circle = rhino3dm.Circle(pt, random.uniform(1, 4))
    #model.Objects.AddCircle(circle)
    curve = circle.ToNurbsCurve()
    curves.append(curve)

bcurves = compute_rhino3d.Curve.CreateBooleanUnion(curves)

for bcurve in bcurves:
    bcurveObj = rhino3dm.CommonObject.Decode(bcurve)

    extrusion = rhino3dm.Extrusion.Create(bcurveObj, 5, True)
    #model.Objects.AddExtrusion(extrusion)

    brep = extrusion.ToBrep(True)
    meshes = compute_rhino3d.Mesh.CreateFromBrep(brep)
    for i in range(len(meshes)):
        meshObj = rhino3dm.CommonObject.Decode(meshes[i])
        model.Objects.AddMesh(meshObj)


model.Write('mesh.3dm', 5)