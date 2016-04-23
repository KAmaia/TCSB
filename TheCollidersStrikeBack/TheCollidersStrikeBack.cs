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

namespace TheCollidersStrikeBack {
	[KSPAddon ( KSPAddon.Startup.Flight, false )]
	public class TCSBController : MonoBehaviour {
		int frameCount = 0;
		Queue<Vessel> vesselsToUpdateNextFrame = new Queue<Vessel> ( );

		List<Tuple<Part, Collider[]>> partsAndColliders = new List<Tuple<Part, Collider[]>> ( );

		void Start ( ) {
			Debug.Log ( "Starting TheCollisionsStrikeBack Controller" );
			GameEvents.onVesselCreate.Add ( UpdateVesselNextFrame );
			GameEvents.onVesselCreate.Add ( EnumeratePartsAndColliders );
			GameEvents.onNewVesselCreated.Add ( UpdateVesselNextFrame );
			GameEvents.onNewVesselCreated.Add ( EnumeratePartsAndColliders );
			GameEvents.onVesselGoOffRails.Add ( UpdateVesselNextFrame );
			GameEvents.onVesselGoOffRails.Add ( EnumeratePartsAndColliders );
			GameEvents.onVesselWasModified.Add ( UpdateVesselNextFrame );
			GameEvents.onVesselWasModified.Add ( EnumeratePartsAndColliders );

			GameEvents.onCollision.Add ( TestCollisionManager );

			UpdateVesselNextFrame ( FlightGlobals.ActiveVessel );
		}

		void FixedUpdate ( ) {
			if ( FlightGlobals.ready ) {
				if ( frameCount > 0 ) {
					frameCount--;
				}
				else {
					//while ( vesselsToUpdateNextFrame.Count > 0 ) {
					//ReenableCollisions ( vesselsToUpdateNextFrame.Dequeue ( ) );
					ReenableCollisions ( );
					//}
				}
			}
		}
		//so we're only doing this once.
		void EnumeratePartsAndColliders ( Vessel v ) {
			//first clear the list, just to be sure.
			partsAndColliders.Clear ( );
			foreach ( Part p in v.parts ) {
				try {
					partsAndColliders.Add ( new Tuple<Part, Collider[]> ( p, p.GetComponentsInChildren<Collider> ( ) ) );
				}
				catch ( Exception ex ) {
					//fail silently for pwings;
					Collider[] pwingCollider = new Collider[1]{ p.collider };
					partsAndColliders.Add ( new Tuple<Part, Collider[]> ( p, pwingCollider ) );
				}
			}
		}

		void UpdateVesselNextFrame ( Vessel v ) {
			vesselsToUpdateNextFrame.Enqueue ( v );
			frameCount = 2;
		}

		void TestCollisionManager ( EventReport report ) {
			print ( "========TCSB TESTING!========" );
			print ( report.sender );
			print ( "||||||||TCSB TESTING!||||||||" );

		}


		void ReenableCollisions ( ) {
			for ( int i = 0; i < partsAndColliders.Count; i++ ) {
				Part currentPart = partsAndColliders [ i ].First;
				for ( int j = 0; j < partsAndColliders.Count; j++ ) {
					Part secondPart = partsAndColliders [ j ].First;
					if ( i == j ) {
						continue;
					}
					if ( currentPart.parent == secondPart || secondPart.parent == currentPart ) {
						continue;
					}
					EnableColliders ( partsAndColliders [ i ].Second, partsAndColliders [ j ].Second );
				}
			}
		}

	 
		// Ferram's Original Method.
		//		void ReenableCollisions ( Vessel v ) {
		//			List<Tuple<Part, Collider[]>> partsAndColliders = new List<Tuple<Part, Collider[]>> ( );
		//			for ( int i = 0; i < v.Parts.Count; i++ ) {
		//				//get the current part
		//				Part p = v.Parts [ i ];
		//				Collider[] colliders;
		//				try {
		//					colliders = p.GetComponentsInChildren<Collider> ( );
		//				}
		//				catch ( Exception e ) {
		//					//Fail silently because it's the only way to avoid issues with pWings
		//					//Debug.LogException(e);
		//					colliders = new Collider[1] { p.collider };
		//				}
		//				partsAndColliders.Add ( new Tuple<Part, Collider[]> ( p, colliders ) );

		//				Debug.Log ( "Part: " + p.partInfo.title + " Collider Count: " + colliders.Length );
		//			}
		//			for ( int i = 0; i < partsAndColliders.Count; i++ ) {
		//				Part currentPart = partsAndColliders [ i ].First;
		//				Collider[] currentColliderArray = partsAndColliders [ i ].Second;
		//				for ( int j = 0; j < partsAndColliders.Count; j++ ) {
		//					Part secondPart = partsAndColliders [ j ].First;
		//					if ( i == j )
		//						continue;
		//					if ( currentPart.parent == secondPart || secondPart.parent == currentPart ) {
		//						continue;
		//					}
		//					Collider[] secondColliderArray = partsAndColliders [ j ].Second;
		//
		//					EnableColliders ( currentColliderArray, secondColliderArray );
		//				}
		//			}
		//
		//		}

		//Make sure that all colliders in array1 can collide with all colliders in array2
		void EnableColliders ( Collider[] array1, Collider[] array2 ) {
			for ( int i = 0; i < array1.Length; i++ ) {
				for ( int j = 0; j < array2.Length; j++ ) {
					Physics.IgnoreCollision ( array1 [ i ], array2 [ j ], false );
				}
			}
		}


		void OnDestroy ( ) {
			GameEvents.onVesselCreate.Remove ( UpdateVesselNextFrame );
			GameEvents.onNewVesselCreated.Remove ( UpdateVesselNextFrame );
			GameEvents.onVesselGoOffRails.Remove ( UpdateVesselNextFrame );
			GameEvents.onVesselWasModified.Remove ( UpdateVesselNextFrame );
			GameEvents.onVesselWasModified.Remove ( EnumeratePartsAndColliders );
			GameEvents.onVesselGoOffRails.Remove ( EnumeratePartsAndColliders );
			GameEvents.onVesselCreate.Remove ( EnumeratePartsAndColliders );
			GameEvents.onNewVesselCreated.Remove ( EnumeratePartsAndColliders );
		}
	}
}
