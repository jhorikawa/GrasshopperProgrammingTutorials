import rhinoinside
rhinoinside.load()
import System
import Rhino
import streamlit as st
import matplotlib.pyplot as plt
import math


st.write("""
# RhinoInside for CPython with Streamlit
""")

ang = st.slider("Step Angle", 0.0, 90.0, 30.0, 0.1)

xarray = []
yarray = []
for i in range(100):
    pos = Rhino.Geometry.Point3d(i * 0.1, 0.0, 0.0)
    pt = Rhino.Geometry.Point(pos)
    pt.Rotate(math.radians(ang * i), Rhino.Geometry.Vector3d.ZAxis, Rhino.Geometry.Point3d.Origin)
    xarray.append(pt.Location.X)
    yarray.append(pt.Location.Y)

fig, ax = plt.subplots()
ax.plot(xarray, yarray)

st.pyplot(fig)