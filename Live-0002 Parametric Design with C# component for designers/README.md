# Parametric Design with C# component for designers

## Requirements

- Rhinoceros 6
- A bit of coding experience.

## References
- Populate 2D sampling algorithm: https://www.grasshopper3d.com/forum/topics/
- Value Remapping algorithm: https://stackoverflow.com/questions/3451553/value-remapping
- RhinoCommon API: https://developer.rhino3d.com/api/RhinoCommon/html/R_Project_RhinoCommon.htm

## Create a design tool with C#

In this session we are going to create a custom component for Grasshopper with C# to populate geometry with attractor points input in order to create gradational scattered points.

![alt text](imgs/img1.png)

![alt text](imgs/img2.png)

![alt text](imgs/img3.png)

![alt text](imgs/img4.png)

## Algorithm explained

![alt text](imgs/sketch.png)

## Step 1. Setup C# component input

### Inputs:
- <strong>brep (Brep / Item):</strong> Geometry to place points on.
- <strong>attractors (Point3d / List):</strong> Attractor points to create attracting forces to control point scattering.
- <strong>num (Integer / Item):</strong> Number of points.
- <strong>ite (Integer / Item):</strong> Scattering iteration.
- <strong>attractor_rad (Double / Item):</strong> Attractor point affecting radius.
- <strong>attractor_pow (Double / Item):</strong> Attractor point power (-/+).

### Output:

- <strong>A:</strong> Ouput scattered points.

###

![alt text](imgs/component.png)

## Step 2. Add necessary namespace

Add a namespace to use Rhino collection in order to use a convenient class. Specifically we are going to use Point3dList class.

```csharp
using Rhino.Collections;
```

## Step 3. Add resusable functions

Value remapping function.

```csharp
  double Remap(double val, double smin, double smax, double tmin, double tmax){
    return (val - smin) / (smax - smin) * (tmax - tmin) + tmin;
  }
```

A function to create random point on top of input brep.

```csharp
  Point3d CreateRandomPoint(Random rnd, Brep brep, BoundingBox bbox){
    var x = Remap(rnd.NextDouble(), 0.0, 1.0, bbox.Min.X, bbox.Max.X);
    var y = Remap(rnd.NextDouble(), 0.0, 1.0, bbox.Min.Y, bbox.Max.Y);
    var z = Remap(rnd.NextDouble(), 0.0, 1.0, bbox.Min.Z, bbox.Max.Z);

    return brep.ClosestPoint(new Point3d(x, y, z));
  }
```

## Step 4. Try creating random points on given geometry

```csharp
    // Create emply point list with Point3dList.
    var points = new Point3dList();

    // Check that brep is not null.
    if(brep != null){
      // Create random class.
      var rnd = new Random();
      // Create a bounding box from input brep.
      var bbox = brep.GetBoundingBox(true);

      // Create number of random points on given brep geometry.
      for(int i = 0; i < num; i++){
        var rndpt = CreateRandomPoint(rnd, brep, bbox);
        points.Add(rndpt);
      }
    }
    // Output points.
    A = points; 
```

## Step 5. Create populate geometry function

```csharp
var points = new Point3dList();
    if(brep != null){
      // Create attractor point list with Poin3dList.
      var attracts = new Point3dList(attractors);
      var rnd = new Random();
      var bbox = brep.GetBoundingBox(true);

      for(int i = 0; i < num; i++){
        
        if(points.Count == 0){ // Create first random point.
          var rndpt = CreateRandomPoint(rnd, brep, bbox);
          points.Add(rndpt);
        }else{ // Create scattered random points.
          double fdist = -1; // A variable to store furthest distance from exisisting points.
          Point3d fpos = new Point3d(); // A variable to store furthest point from existing points.

          // Find furthest point from existing points by creating number of random points.
          for(int t = 0; t < Math.Max(Math.Min(ite, i), 10); t++){
            // Create random point
            var nrndpt = CreateRandomPoint(rnd, brep, bbox);

            // Find closest point from existing points with random point.
            var nindex = points.ClosestIndex(nrndpt);
            var npos = points[nindex];
            var ndist = npos.DistanceTo(nrndpt);

            // Find furthest random point
            if(fdist < ndist){
              fdist = ndist;
              fpos = nrndpt;
            }
          }
          // Add a furthest random point to points list.
          points.Add(fpos);
        }
      }
    }

    A = points;

```

## Step 6. Use attractor points to modify scattered points.

```csharp

  private void RunScript(Brep brep, List<Point3d> attractors, int num, int ite, double attractor_rad, double attractor_pow, ref object A)
  {
    var points = new Point3dList();
    
    if(brep != null){
      var attracts = new Point3dList(attractors);
      var rnd = new Random();
      var bbox = brep.GetBoundingBox(true);

      for(int i = 0; i < num; i++){

        if(points.Count == 0){
          var rndpt = CreateRandomPoint(rnd, brep, bbox);
          points.Add(rndpt);
        }else{

          double fdist = -1;
          Point3d fpos = new Point3d();
          for(int t = 0; t < Math.Max(Math.Min(ite, i), 10); t++){
            var nrndpt = CreateRandomPoint(rnd, brep, bbox);
            var nindex = points.ClosestIndex(nrndpt);
            var npos = points[nindex];

            // Create attractor weight.
            var nattractid = attracts.ClosestIndex(nrndpt);
            var nattractpos = attracts[nattractid];
            var mindist = attractor_rad;
            var pow = attractor_pow;
            var nattractdist = Math.Pow(Remap(Math.Min(nattractpos.DistanceTo(nrndpt), mindist), 0, mindist, 0, 1.0), pow);

            // Multipy distance with attractor weight.
            var ndist = npos.DistanceTo(nrndpt) * nattractdist;

            if(fdist < ndist){
              fdist = ndist;
              fpos = nrndpt;
            }
          }
          points.Add(fpos);
        }
      }
    }

    A = points;
  }

```

## Final Code

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Collections;

  private void RunScript(Brep brep, List<Point3d> attractors, int num, int ite, double attractor_rad, double attractor_pow, ref object A)
  {
    var points = new Point3dList();
    
    if(brep != null){
      var attracts = new Point3dList(attractors);
      var rnd = new Random();
      var bbox = brep.GetBoundingBox(true);

      for(int i = 0; i < num; i++){

        if(points.Count == 0){
          var rndpt = CreateRandomPoint(rnd, brep, bbox);
          points.Add(rndpt);
        }else{

          double fdist = -1;
          Point3d fpos = new Point3d();
          for(int t = 0; t < Math.Max(Math.Min(ite, i), 10); t++){
            var nrndpt = CreateRandomPoint(rnd, brep, bbox);
            var nindex = points.ClosestIndex(nrndpt);
            var npos = points[nindex];

            // Create attractor weight.
            var nattractid = attracts.ClosestIndex(nrndpt);
            var nattractpos = attracts[nattractid];
            var mindist = attractor_rad;
            var pow = attractor_pow;
            var nattractdist = Math.Pow(Remap(Math.Min(nattractpos.DistanceTo(nrndpt), mindist), 0, mindist, 0, 1.0), pow);

            // Multipy distance with attractor weight.
            var ndist = npos.DistanceTo(nrndpt) * nattractdist;

            if(fdist < ndist){
              fdist = ndist;
              fpos = nrndpt;
            }
          }
          points.Add(fpos);
        }
      }
    }

    A = points;
  }

  // <Custom additional code> 
  double Remap(double val, double smin, double smax, double tmin, double tmax){
    return (val - smin) / (smax - smin) * (tmax - tmin) + tmin;
  }

  Point3d CreateRandomPoint(Random rnd, Brep brep, BoundingBox bbox){
    var x = Remap(rnd.NextDouble(), 0.0, 1.0, bbox.Min.X, bbox.Max.X);
    var y = Remap(rnd.NextDouble(), 0.0, 1.0, bbox.Min.Y, bbox.Max.Y);
    var z = Remap(rnd.NextDouble(), 0.0, 1.0, bbox.Min.Z, bbox.Max.Z);

    return brep.ClosestPoint(new Point3d(x, y, z));
  }
}

```

## Create a design with this cutom tool

![alt text](imgs/component2.png)

![alt text](imgs/img5.png)

![alt text](imgs/img6.png)