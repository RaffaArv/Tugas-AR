using UnityEngine;
using UnityEngine.SceneManagement; // Penting: Namespace ini diperlukan untuk memuat scene

// Skrip ini hanya memiliki satu tanggung jawab: mengelola perpindahan antar scene.
// Skrip ini sangat ringan dan ideal untuk dipasang pada tombol UI.

public class SceneLoader : MonoBehaviour
{
    // Fungsi publik ini dapat dihubungkan ke event OnClick() pada tombol UI.
    // Parameter 'ScanAr' diisi di Inspector tombol, isinya adalah nama scene tujuan.
    public void LoadScene(string ScanAr)
    {
        // 1. Pastikan nama scene tidak kosong
        if (string.IsNullOrEmpty(ScanAr))
        {
            Debug.LogError("SceneLoader: Nama scene tidak boleh kosong!");
            return;
        }

        // 2. Pastikan scene yang dituju sudah ditambahkan ke Build Settings (File > Build Settings).
        Debug.Log("SceneLoader: Memuat Scene: " + ScanAr);
        SceneManager.LoadScene(ScanAr);
    }

    // Anda bisa menambahkan fungsi lain di sini, misalnya:
    /*
    public void QuitApplication()
    {
        Debug.Log("Aplikasi Keluar.");
        Application.Quit();
    }
    */
}