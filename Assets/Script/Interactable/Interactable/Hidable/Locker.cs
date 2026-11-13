using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Locker : Hidable
{
    [Header("Sound Clips")]
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float openVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float closeVolume = 1f;

    [Header("Timing")]
    [Tooltip("หน่วงกี่วินาทีหลังเข้าตู้ ก่อนเล่นเสียงปิดประตู (0 = ไม่เล่นเสียงปิดตอนเข้าตู้)")]
    [SerializeField, Min(0f)] private float closeDelayAfterEnter = 0.6f;

    [Header("Variation (optional)")]
    [Tooltip("สุ่ม pitch เล็กน้อยให้เสียงไม่ซ้ำเดิมทุกครั้ง (1,1 = ปิดการสุ่ม)")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("3D Sound")]
    [Range(0f, 1f)]
    [Tooltip("0 = เสียง 2D (ดังเท่ากันทุกที่), 1 = เสียง 3D (เบาลงเมื่ออยู่ไกลตู้)")]
    [SerializeField] private float spatialBlend = 1f;

    private AudioSource lockerAudioSource;

    private AudioSource LockerAudio
    {
        get
        {
            if (lockerAudioSource == null)
            {
                lockerAudioSource = GetComponent<AudioSource>();
                lockerAudioSource.playOnAwake = false;
                lockerAudioSource.spatialBlend = spatialBlend;
            }
            return lockerAudioSource;
        }
    }

    // เข้าตู้: เปิดประตู แล้วปิดประตูตามหลัง
    protected override void OnEnterHiding(IHidingAgent agent)
    {
        PlayOpenSound();

        if (closeDelayAfterEnter > 0f)
            Invoke(nameof(PlayCloseSound), closeDelayAfterEnter);
    }

    // ออกจากตู้: เปิดประตู
    protected override void OnExitHiding(IHidingAgent agent)
    {
        CancelInvoke(nameof(PlayCloseSound));
        PlayOpenSound();
    }

    /// <summary>เสียงเปิดตู้</summary>
    public void PlayOpenSound()
    {
        PlaySound(openClip, openVolume);
    }

    /// <summary>เสียงปิดตู้</summary>
    public void PlayCloseSound()
    {
        PlaySound(closeClip, closeVolume);
    }

    /// <summary>ปรับ volume ของทั้งเสียงเปิดและปิดพร้อมกัน (เช่น จากเมนู Options)</summary>
    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        openVolume = value;
        closeVolume = value;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning($"{name}: ยังไม่ได้ใส่ AudioClip ใน Locker", this);
            return;
        }

        LockerAudio.pitch = Random.Range(pitchRange.x, pitchRange.y);
        LockerAudio.PlayOneShot(clip, volume);
    }
}
