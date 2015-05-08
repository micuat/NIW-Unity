/* VRChangeManipulationDeviceIPSISample
 * Written by MiddleVR.
 *
 * This code is given as an example. You can do whatever you want with it
 * without any restriction.
 */

using UnityEngine;
using System.Collections;

using MiddleVR_Unity3D;

/// <summary>
/// Iterate through physics rigid bodies and make them manipulated by a given Haption manipulation device.
/// 
/// The purpose of this class is only to illustrate how to change dynamically
/// the rigid body being manipulated by a Haption (IPSI) manipulation device.
/// 
/// Usage:
/// press keyboard keys 'h' and 'c'.
/// The keys are this meaning: 'h' stands for 'haptics' and 'c' for 'change'.
///
/// Programming note: in order to work, this script must be executed after the
/// script that create a rigid body.
/// </summary>
public class VRChangeManipulationDeviceIPSISample : MonoBehaviour {

    #region Member Variables

    [SerializeField]
    private int m_ManipulationDeviceId = 0;

    private int m_PhysicsBodyId = -1;

    #endregion

    #region MonoBehaviour Member Functions

    protected void Update()
    {
        if (MiddleVR.VRDeviceMgr != null &&
            MiddleVR.VRDeviceMgr.IsKeyPressed(MiddleVR.VRK_H) &&
            MiddleVR.VRDeviceMgr.IsKeyToggled(MiddleVR.VRK_C))
        {
            if (MiddleVR.VRPhysicsMgr == null)
            {
                MiddleVRTools.Log(0, "[X] VRChangeManipulationDeviceIPSISample: No PhysicsManager found.");
                enabled = false;
                return;
            }

            vrPhysicsEngine physicsEngine = MiddleVR.VRPhysicsMgr.GetPhysicsEngine();

            if (physicsEngine == null)
            {
                return;
            }

            uint bodiesNb = physicsEngine.GetBodiesNb();
            m_PhysicsBodyId = (m_PhysicsBodyId + 1) % ((int)bodiesNb);

            MiddleVRTools.Log(0, "[+] VRChangeManipulationDeviceIPSISample: proposed body id: " +
                m_PhysicsBodyId + ".");

            vrPhysicsBody physicsBody = physicsEngine.GetBody((uint)m_PhysicsBodyId);
            if (physicsBody != null && physicsBody.IsA("PhysicsBodyIPSI"))
            {
                var kernel = MiddleVR.VRKernel;

                var objId = physicsBody.GetId();

                // SetAttachedToAManipDevice (do an attachment).
                var setAttachedToAManipDeviceValues = vrValue.CreateList();
                setAttachedToAManipDeviceValues.AddListItem(objId);
                setAttachedToAManipDeviceValues.AddListItem(true);

                kernel.ExecuteCommand(
                    "Haption.IPSI.SetAttachedToAManipulationDevice",
                    setAttachedToAManipDeviceValues);

                // The previous manipulated physics body (if any), will be
                // automatically marked as not-manipulated.
                // SetManipulationDevice (only attachment).
                var setManipDeviceValues = vrValue.CreateList();
                setManipDeviceValues.AddListItem(objId);
                setManipDeviceValues.AddListItem(m_ManipulationDeviceId);

                kernel.ExecuteCommand(
                    "Haption.IPSI.SetManipulationDeviceId",
                    setManipDeviceValues);

                MiddleVRTools.Log(0, "[+] VRChangeManipulationDeviceIPSISample: attached '" +
                    physicsBody.GetName() + "' to the manipulation device '" +
                    m_ManipulationDeviceId + "'.");
            }
        }
    }

    #endregion
}
