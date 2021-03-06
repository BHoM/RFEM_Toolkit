/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.Constraints;

using rf = Dlubal.RFEM5;
using BH.oM.Structure.MaterialFragments;

namespace BH.Adapter.RFEM
{
    public partial class RFEMAdapter
    {

        /***************************************************/
        /**** Private methods                           ****/
        /***************************************************/

        private List<Panel> ReadPanels(List<string> ids = null)
        {
            List<Panel> panelList = new List<Panel>();
            ISurfaceProperty surfaceProperty;

            if (ids == null)
            {
                foreach (rf.Surface surface in modelData.GetSurfaces())
                {
                    if(surface.GeometryType != rf.SurfaceGeometryType.PlaneSurfaceType)
                        Engine.Reflection.Compute.RecordError("Only plane surface types are supported at the moment");

                    List<Edge> edgeList = GetEdgesFromRFEMSurface(surface);

                    IMaterialFragment material = modelData.GetMaterial(surface.MaterialNo, rf.ItemAt.AtNo).GetData().FromRFEM();

                    if (surface.StiffnessType == rf.SurfaceStiffnessType.StandardStiffnessType)
                    {
                        surfaceProperty = Engine.Structure.Create.ConstantThickness(surface.Thickness.Constant, material);
                    }
                    else if (surface.StiffnessType == rf.SurfaceStiffnessType.OrthotropicStiffnessType)
                    {
                        rf.ISurface s = modelData.GetSurface(surface.No, rf.ItemAt.AtNo);
                        rf.IOrthotropicThickness ortho = s.GetOrthotropicThickness();
                        rf.SurfaceStiffness stiffness = ortho.GetData();
                        surfaceProperty = stiffness.FromRFEM(material);
                    }
                    else
                    {
                        surfaceProperty = null;
                        Engine.Reflection.Compute.RecordError("could not create surface property of type " + surface.StiffnessType.ToString());
                    }
                    
                    List<Opening> openings = null;
                    Panel panel = Engine.Structure.Create.Panel(edgeList, openings, surfaceProperty);

                    panelList.Add(panel);
                }
            }
            else
            {
                foreach (string id in ids)
                {
                    rf.Surface surface = modelData.GetSurface(Int32.Parse(id), rf.ItemAt.AtNo).GetData();
                    if (surface.GeometryType != rf.SurfaceGeometryType.PlaneSurfaceType)
                        Engine.Reflection.Compute.RecordError("Only plane surface types are supported at the moment");

                    List<Edge> edgeList = GetEdgesFromRFEMSurface(surface);

                    IMaterialFragment material = modelData.GetMaterial(surface.MaterialNo, rf.ItemAt.AtNo).GetData().FromRFEM();

                    rf.ISurface s = modelData.GetSurface(surface.No, rf.ItemAt.AtNo);
                    rf.IOrthotropicThickness ortho = s.GetOrthotropicThickness();
                    rf.SurfaceStiffness stiffness = ortho.GetData();

                    surfaceProperty = stiffness.FromRFEM(material);

                    List<Opening> openings = null;
                    Panel panel = Engine.Structure.Create.Panel(edgeList, openings, surfaceProperty);

                    panelList.Add(panel);
                }
            }

            return panelList;
        }

        /***************************************************/

        private List<Edge> GetEdgesFromRFEMSurface(rf.Surface surface)
        {
            List<Edge> edgeList = new List<Edge>();
            string boundaryString = modelData.GetSurface(surface.No, rf.ItemAt.AtNo).GetData().BoundaryLineList; 

            List<int> boundaryLineIds = GetIdListFromString(boundaryString);


            foreach (int edgeId in boundaryLineIds)
            {
                List<oM.Geometry.Point> ptsInEdge = new List<oM.Geometry.Point>();
                string nodeIdString = modelData.GetLine(edgeId, rf.ItemAt.AtNo).GetData().NodeList;
                List<int> nodeIds = GetIdListFromString(nodeIdString);

                foreach (int ptId in nodeIds)
                {
                    rf.Node rfNode = modelData.GetNode(ptId, rf.ItemAt.AtNo).GetData();
                    ptsInEdge.Add(new oM.Geometry.Point() { X = rfNode.X, Y = rfNode.Y, Z = rfNode.Z });
                }
                edgeList.Add(Engine.Structure.Create.Edge(Engine.Geometry.Create.Polyline(ptsInEdge), null, ""));
            }

            return edgeList;
        }

        private List<int> GetIdListFromString(string idsAsString)
        {
            //NOTE: the below only works if RFEM does not use a mix of ',' and '-' delimiters !!
            List<int> idList = new List<int>();

            if(idsAsString.Contains('-') & idsAsString.Contains(','))
            {
                foreach(string part in idsAsString.Split(','))
                {
                    if (!part.Contains('-'))
                    {
                        idList.Add(System.Convert.ToInt32(part));
                    }
                    else
                    {
                        List<int> startEnd = part.Split('-').ToList().ConvertAll(s => Int32.Parse(s));
                        idList.AddRange(Enumerable.Range(startEnd[0], startEnd[1] - startEnd[0] + 1));
                    }
                }
            }
            else if (idsAsString.Contains(','))
            {
                idList = idsAsString.Split(',').ToList().ConvertAll(s => Int32.Parse(s));
            }
            else if (idsAsString.Contains('-'))
            {
                List<int> startEnd = idsAsString.Split('-').ToList().ConvertAll(s => Int32.Parse(s));
                idList = Enumerable.Range(startEnd[0], startEnd[1] - startEnd[0] + 1).ToList();
            }
            else
            {
                idList.Add(System.Convert.ToInt32(idsAsString));
            }

            return idList;
        }
    }
}

