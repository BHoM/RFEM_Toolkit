﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Common.Materials;

using rf = Dlubal.RFEM5;
using BH.Engine.RFEM;
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
            rf.Line line;
            ISurfaceProperty sectionProperty;


            if (ids == null)
            {
                foreach (rf.Surface surface in modelData.GetSurfaces())
                {
                    //string[] boundaryLineIds = modelData.GetSurface(surface.No, rf.ItemAt.AtNo).GetData().BoundaryLineList.Split(',');
                    rf.Point3D[,] controlePts = modelData.GetSurface(surface.No, rf.ItemAt.AtNo).GetData().ControlPoints;

                    sectionProperty = BH.Engine.Structure.Create.ConstantThickness(0.1);
                    //line = modelData.GetLine(member.LineNo, rf.ItemAt.AtNo).GetData();

                    //if (!m_sectionDict.TryGetValue(member.StartCrossSectionNo, out sectionProperty))
                    //{
                    //    rf.CrossSection rfSection = modelData.GetCrossSection(member.StartCrossSectionNo, rf.ItemAt.AtNo).GetData();
                    //    rf.Material rfMat = modelData.GetMaterial(rfSection.MaterialNo, rf.ItemAt.AtNo).GetData();
                    //    sectionProperty = rfSection.FromRFEM(rfMat);
                    //    m_sectionDict.Add(member.StartCrossSectionNo, sectionProperty);
                    //}

                    panelList.Add(surface.FromRFEM(sectionProperty));
                }
            }
            else
            {
                foreach (string id in ids)
                {
                    rf.Surface surface = modelData.GetSurface(Int32.Parse(id), rf.ItemAt.AtNo).GetData();
                    sectionProperty = BH.Engine.Structure.Create.ConstantThickness(0.1);

                    //line = modelData.GetLine(member.LineNo, rf.ItemAt.AtNo).GetData();

                    //if (!m_sectionDict.TryGetValue(member.StartCrossSectionNo, out sectionProperty))
                    //{
                    //    rf.CrossSection rfSection = modelData.GetCrossSection(member.StartCrossSectionNo, rf.ItemAt.AtNo).GetData();
                    //    rf.Material rfMat = modelData.GetMaterial(rfSection.MaterialNo, rf.ItemAt.AtNo).GetData();
                    //    sectionProperty = rfSection.FromRFEM(rfMat);
                    //    m_sectionDict.Add(member.StartCrossSectionNo, sectionProperty);
                    //}

                    panelList.Add(surface.FromRFEM(sectionProperty));
                }
            }

            return panelList;
        }

        /***************************************************/

    }
}