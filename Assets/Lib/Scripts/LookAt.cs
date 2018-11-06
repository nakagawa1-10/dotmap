using UnityEngine;

namespace Kosu.UnityLibrary
{
    /// <summary>
    /// 注視ターゲット設定クラス
    /// </summary>
    [ExecuteInEditMode]
    public class LookAt : MonoBehaviour
    {
        [SerializeField]
        private Transform _lookAtTarget;

        [SerializeField]
        private Vector3 _offset;

        [SerializeField]
        private bool _isSmoothLookAt;

        public void SetLookAtTarget(Transform lookAtTarget)
        {
            _lookAtTarget = lookAtTarget;
        }

        private void Update()
        {
            if (_lookAtTarget == null)
            {
                return;
            }

            if (_isSmoothLookAt && Application.isPlaying)
            {
                SmoothLookAtTarget();
            }
            else
            {
                LookAtTarget();
            }
        }

        private void LookAtTarget()
        {
            transform.LookAt(_lookAtTarget.position + _offset);
        }

        private void SmoothLookAtTarget()
        {
            var dir = _lookAtTarget.position - transform.position + _offset;
            Quaternion toRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRot, Time.deltaTime);
        }
    }
}
