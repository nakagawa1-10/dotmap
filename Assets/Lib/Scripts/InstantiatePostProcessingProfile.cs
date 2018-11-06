using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections.Generic;

namespace Kosu.UnityLibrary
{
    /// <summary>
    /// Use this component to dynamically create a PostProcessingBehaviour and instantiate a PostProcessingProfile on a Camera
    /// This allows you to dynamically modify at runtime the PostProcessingProfile, without modifying the asset.
    /// This component keeps track of the Profile and Instances. This means that if 2 different camera use the same Profile, they will use the same Instance.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class InstantiatePostProcessingProfile : MonoBehaviour
    {

        [SerializeField]
        private PostProcessingProfile m_Profile = null;

        private static Dictionary<PostProcessingProfile, PostProcessingProfile> ms_RefToInstance = new Dictionary<PostProcessingProfile, PostProcessingProfile>();

        private static PostProcessingProfile AssignProfile(PostProcessingProfile reference)
        {
            if (reference == null)
            { return null; }

            // keep track of the profile and instances: only 1 instance is created per profile
            // (event if multiple cameras share 1 profile)
            if (!ms_RefToInstance.ContainsKey(reference))
            {
                var profileInstance = Object.Instantiate(reference);
                QualitySettingsAdjustments(profileInstance);
                ms_RefToInstance.Add(reference, profileInstance);
            }

            return ms_RefToInstance[reference];
        }

        private void OnEnable()
        {
            if (m_Profile)
            {
                var ppb = gameObject.GetOrAddComponent<PostProcessingBehaviour>();
                Debug.Assert(ppb);
                //ppb.profile = AssignProfile(m_Profile);
                ppb.profile = Instantiate(m_Profile);
            }

            Object.Destroy(this);
        }

        private static void QualitySettingsAdjustments(PostProcessingProfile profile)
        {
            // Here you can adjust the PostProcessingProfile just after instantiation.
            // For example you can change it according to the Quality Settings.
            // Like here we disable Motion Blur for low quality.
            if (QualitySettings.GetQualityLevel() < 1)
            { profile.motionBlur.enabled = false; }
        }

    }
}
