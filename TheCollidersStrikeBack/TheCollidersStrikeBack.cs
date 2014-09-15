/* Copyright (c) 2014, Michael Ferrara
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheCollidersStrikeBack
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TCSBController : MonoBehaviour
    {
        bool currentVesselCollisionsEnabled = false;

        void Start()
        {
            currentVesselCollisionsEnabled = false;
            Debug.Log("Starting TheCollisionsStrikeBack Controller");
            GameEvents.onVesselCreate.Add(ReenableCollisions);
            GameEvents.onNewVesselCreated.Add(ReenableCollisions);
            GameEvents.onVesselGoOffRails.Add(ReenableCollisions);
            GameEvents.onVesselWasModified.Add(ReenableCollisions);
        }

        void FixedUpdate()
        {
            if (FlightGlobals.ready && !currentVesselCollisionsEnabled)
                ReenableCollisions(FlightGlobals.ActiveVessel);
        }

        void ReenableCollisions(Vessel v)
        {
            List<Collider[]> colliderArrays = new List<Collider[]>();
            for(int i = 0; i < v.Parts.Count; i++)
            {
                Part p = v.Parts[i];

                Collider[] colliders;
                try
                {
                    colliders = p.GetComponentsInChildren<Collider>();
                }
                catch (Exception e)
                {
                    //Fail silently because it's the only way to avoid issues with pWings
                    //Debug.LogException(e);
                    colliders = new Collider[1] { p.collider };
                }
                colliderArrays.Add(colliders);
                Debug.Log("Part: " + p.partInfo.title + " Collider Count: " + colliders.Length);
            }
            for(int i = 0; i < colliderArrays.Count; i++)
            {
                Collider[] currentColliderArray = colliderArrays[i];
                for(int j = 0; j < colliderArrays.Count; j++)
                {
                    if (i == j)
                        continue;

                    Collider[] secondColliderArray = colliderArrays[j];

                    EnableColliders(currentColliderArray, secondColliderArray);
                }
            }
        }

        //Make sure that all colliders in array1 can collide with all colliders in array2
        void EnableColliders(Collider[] array1, Collider[] array2)
        {
            for(int i = 0; i < array1.Length; i++)
                for (int j = 0; j < array2.Length; j++)
                {
                    Physics.IgnoreCollision(array1[i], array2[j], false);
                }
        }

        void OnDestroy()
        {
            GameEvents.onVesselCreate.Remove(ReenableCollisions);
            GameEvents.onNewVesselCreated.Remove(ReenableCollisions);
            GameEvents.onVesselGoOffRails.Remove(ReenableCollisions);
            GameEvents.onVesselWasModified.Remove(ReenableCollisions);
        }
    }
}
