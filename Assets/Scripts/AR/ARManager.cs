using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using System.Linq; // Penting: Namespace ini diperlukan agar fungsi .First() dan LINQ lainnya bekerja

// Skrip ini berfungsi untuk:
// 1. Menginisialisasi data komponen AR (CPU, RAM, dll.)
// 2. Mendaftarkan event (Found/Lost) pada semua Image Target di Scene, menggunakan API Vuforia terbaru (melalui DefaultObserverEventHandler).
// 3. Menampilkan model 3D dan info UI ketika marker terdeteksi.
// 4. Menyembunyikan model 3D dan info UI ketika marker hilang.

public class ARManager : MonoBehaviour
{
    // Struktur data untuk komponen AR yang akan dimuat
    [System.Serializable]
    public class ComponentData
    {
        public string id;           // Nama marker (harus sama dengan nama Image Target di Vuforia)
        public string name;         // Nama komponen (untuk ditampilkan di UI)
        public string prefab;       // Nama prefab (harus ada di folder Resources/Prefabs)
        public string description;  // Penjelasan komponen
    }

    public List<ComponentData> components = new List<ComponentData>();

    void Start()
    {
        LoadDefaultData();

        // [Perbaikan CS0618] Menggunakan FindObjectsByType() sesuai rekomendasi Unity terbaru
        var targets = FindObjectsByType<ImageTargetBehaviour>(FindObjectsSortMode.None);
        
        Debug.Log("ARManager: " + targets.Length + " Image Targets ditemukan di Scene.");

        // Iterasi melalui setiap Image Target yang ditemukan
        foreach (var target in targets)
        {
            // [Perbaikan CS1061] Ambil DefaultObserverEventHandler untuk mendaftarkan event
            // Vuforia modern menggunakan komponen EventHandler ini, bukan event langsung di ImageTargetBehaviour.
            var eventHandler = target.GetComponent<DefaultObserverEventHandler>();

            if (eventHandler != null)
            {
                // Mendaftarkan fungsi OnTargetFound dan OnTargetLost ke event handler
                eventHandler.OnTargetFound.AddListener(OnTargetFound);
                eventHandler.OnTargetLost.AddListener(OnTargetLost);
                Debug.Log("ARManager: Event listener berhasil didaftarkan pada target: " + target.TargetName);
            }
            else
            {
                Debug.LogError("ARManager: DefaultObserverEventHandler TIDAK ditemukan pada target: " + target.TargetName + ". Pastikan komponen ini ada pada semua Image Target.");
            }
        }
    }

    // Dipanggil ketika target ditemukan
    private void OnTargetFound()
    {
        // Mendapatkan ObserverBehaviour dari komponen DefaultObserverEventHandler yang memicu event.
        // Fungsi LINQ (.First) digunakan untuk melacak instance mana yang memanggil event ini.
        DefaultObserverEventHandler eventHandler = FindObjectsOfType<DefaultObserverEventHandler>().First(h => h.OnTargetFound.GetPersistentEventCount() > 0 && h.OnTargetFound.GetPersistentTarget(0) == this);

        if (eventHandler != null)
        {
            ObserverBehaviour behaviour = eventHandler.GetComponent<ObserverBehaviour>();
            string targetId = behaviour.TargetName.ToLower();
            Debug.Log("Target Ditemukan: " + targetId);
            ShowComponentForTarget(behaviour, targetId, true);
        }
    }

    // Dipanggil ketika target hilang
    private void OnTargetLost()
    {
        // Mendapatkan ObserverBehaviour dari komponen DefaultObserverEventHandler yang memicu event.
        DefaultObserverEventHandler eventHandler = FindObjectsOfType<DefaultObserverEventHandler>().First(h => h.OnTargetLost.GetPersistentEventCount() > 0 && h.OnTargetLost.GetPersistentTarget(0) == this);

        if (eventHandler != null)
        {
            ObserverBehaviour behaviour = eventHandler.GetComponent<ObserverBehaviour>();
            string targetId = behaviour.TargetName.ToLower();
            Debug.Log("Target Hilang: " + targetId);
            ShowComponentForTarget(behaviour, targetId, false);
            
            // Sembunyikan info UI ketika target hilang
            // Asumsi ComponentInfo adalah Singleton
            if (ComponentInfo.Instance != null)
            {
                ComponentInfo.Instance.HideInfo();
            }
        }
    }

    // Memuat data konfigurasi komponen AR default
    void LoadDefaultData()
    {
        components = new List<ComponentData>()
        {
            new ComponentData(){
                id = "CPU", 
                name = "Processor (CPU)",
                prefab = "CPU_Prefab",
                description = "CPU adalah otak komputer yang menjalankan instruksi, bertanggung jawab untuk semua pemrosesan utama."
            },
            new ComponentData(){
                id = "Ram", 
                name = "RAM",
                prefab = "RAM_Prefab",
                description = "RAM menyimpan data sementara yang sedang digunakan oleh program. Kecepatan dan kapasitasnya sangat mempengaruhi kinerja."
            }
            // Komponen GPU telah dihapus sesuai permintaan
        };
    }

    // Mengontrol penampilan model 3D (instantiate atau set active/deactive)
    void ShowComponentForTarget(ObserverBehaviour behaviour, string targetId, bool show)
    {
        ComponentData data = components.Find(x => x.id.ToLower() == targetId);
        
        if (data == null)
        {
            Debug.LogWarning("Tidak ada data konfigurasi untuk marker: " + targetId);
            return;
        }

        // Cari model 3D yang sudah di-instantiate (sudah ada)
        Transform existing = behaviour.transform.Find(data.prefab);
        
        if (show) // LOGIKA MENAMPILKAN / MENGAKTIFKAN
        {
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
            }
            else
            {
                // Muat dan buat instance model 3D (jika belum ada)
                GameObject prefab = Resources.Load<GameObject>("Prefabs/" + data.prefab);
                if (prefab == null)
                {
                    Debug.LogError("Prefab tidak ditemukan di Resources/Prefabs/: " + data.prefab);
                    return;
                }

                GameObject go = Instantiate(prefab, behaviour.transform);
                go.name = data.prefab;
                // Atur posisi/rotasi/skala
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * 0.3f; 
            }
            
            // Tampilkan info UI setelah model dipastikan ada
            if (ComponentInfo.Instance != null)
            {
                ComponentInfo.Instance.ShowInfo(data);
            }
        }
        else // LOGIKA MENYEMBUNYIKAN / MENONAKTIFKAN
        {
            if (existing != null)
            {
                existing.gameObject.SetActive(false);
            }
        }
    }
}