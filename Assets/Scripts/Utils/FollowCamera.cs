using UnityEngine;

public class FollowCamera : MonoBehaviour {
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(2.85f,1.98f,4.445f);
    [SerializeField] private float _smoothSpeed = 5f;

    // Lệch điểm nhìn theo trục phải & trục lên của CAMERA
    [SerializeField] private float _lookRightOffset = 0.58f; // dương -> súng lệch TRÁI khung hình
    [SerializeField] private float _lookUpOffset = 0.82f;  // dương -> camera nhìn CAO hơn (súng trông THẤP hơn)

    void LateUpdate() {
        if (_target == null) return;

        // Vị trí camera: giữ như cũ
        Vector3 desiredPos = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * _smoothSpeed);

        // Điểm nhìn: lệch sang phải & lên theo trục của camera
        Vector3 lookPoint = _target.position 
                            + transform.right * _lookRightOffset
                            + transform.up    * _lookUpOffset;

        transform.LookAt(lookPoint);
    }
}
