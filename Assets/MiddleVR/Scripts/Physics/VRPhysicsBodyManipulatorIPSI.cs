/* VRPhysicsBodyManipulatorIPSI
 * MiddleVR
 * (c) MiddleVR
 */

using UnityEngine;
using System;
using MiddleVR_Unity3D;

[AddComponentMenu("MiddleVR/Physics/Body Manipulator IPSI")]
[RequireComponent(typeof(VRPhysicsBody))]
public class VRPhysicsBodyManipulatorIPSI : MonoBehaviour {

    #region Member Variables

    [SerializeField]
    private int m_ManipulationDeviceId = 0;

    private bool m_ScriptStarted = false;
    private string m_PhysicsBodyName = "";

    #endregion

    #region MonoBehaviour Member Functions

    protected void Start()
    {
        m_ScriptStarted = true;

        if (MiddleVR.VRClusterMgr.IsCluster() && !MiddleVR.VRClusterMgr.IsServer())
        {
            enabled = false;
            return;
        }

        vrPhysicsBody physicsBody = GetComponent<VRPhysicsBody>().PhysicsBody;

        if (physicsBody == null)
        {
            MiddleVRTools.Log(0, "[X] PhysicsBodyManipulator: No PhysicsBody found in '"
                + name + "'.");

            enabled = false;

            return;
        }

        // Fix incoherent settings.
        if (physicsBody.IsStatic())
        {
            MiddleVRTools.Log(0, "[X] PhysicsBodyManipulator: The body '" +
                physicsBody.GetName() + "' cannot be manipulated because it is static.");

            // And so we stop to use this script.
            enabled = false;

            return;
        }

        m_PhysicsBodyName = physicsBody.GetName();

        AttachOrDetachBody(true);
    }

    protected void OnEnable()
    {
        if (!m_ScriptStarted)
        {
            // Start() must be executed first to find physics bodies.
            return;
        }

        AttachOrDetachBody(true);
    }

    protected void OnDisable()
    {
        AttachOrDetachBody(false);
    }

    #endregion

    #region VRPhysicsBodyManipulatorIPSI Functions

    protected void AttachOrDetachBody(bool doAttachement)
    {
        VRPhysicsBody physicsBodyComponent = GetComponent<VRPhysicsBody>();

        if (physicsBodyComponent == null)
        {
            return;
        }

        vrPhysicsBody physicsBody = null;

        if (MiddleVR.VRPhysicsMgr != null)
        {
            vrPhysicsEngine physicsEngine = MiddleVR.VRPhysicsMgr.GetPhysicsEngine();

            if (physicsEngine != null)
            {
                // Prefer to find the object by its name so we won't access
                // a dangling pointer when the body was destroyed by MiddleVR.
                physicsBody = physicsEngine.GetBody(m_PhysicsBodyName);
            }
        }

        if (physicsBody == null)
        {
            return;
        }

        var kernel = MiddleVR.VRKernel;

        var physicsBodyId = physicsBody.GetId();

        if (doAttachement)
        {
            // SetManipulationDevice (only attachment).
            var setManipDeviceValues = vrValue.CreateList();
            setManipDeviceValues.AddListItem(physicsBodyId);
            setManipDeviceValues.AddListItem(m_ManipulationDeviceId);

            kernel.ExecuteCommand(
                "Haption.IPSI.SetManipulationDeviceId",
                setManipDeviceValues);
        }

        // SetAttachedToAManipDevice (attachment or detachment).
        var setAttachedToAManipDeviceValues = vrValue.CreateList();
        setAttachedToAManipDeviceValues.AddListItem(physicsBodyId);
        setAttachedToAManipDeviceValues.AddListItem(doAttachement);

        kernel.ExecuteCommand(
            "Haption.IPSI.SetAttachedToAManipulationDevice",
            setAttachedToAManipDeviceValues);
    }

    #endregion
}
