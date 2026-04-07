import React, { useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import authService from '../services/authService';

const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!token) {
      setError('Geçersiz şifre sıfırlama bağlantısı. Token bulunamadı.');
      return;
    }
    if (newPassword.length < 6) {
      setError('Şifre en az 6 karakter olmalıdır.');
      return;
    }
    if (newPassword !== confirmPassword) {
      setError('Şifreler eşleşmiyor.');
      return;
    }

    setLoading(true);
    try {
      await authService.resetPassword(token, newPassword);
      setSuccess(true);
      setTimeout(() => navigate('/login'), 3000);
    } catch (err) {
      setError(
        err?.response?.data?.message ||
        err?.response?.data ||
        'Şifre sıfırlama başarısız. Token geçersiz veya süresi dolmuş olabilir.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (!token) {
    return (
      <div style={{ textAlign: 'center', marginTop: '80px', padding: '20px' }}>
        <h2 style={{ color: 'red' }}>Geçersiz Bağlantı</h2>
        <p>Şifre sıfırlama token'ı bulunamadı.</p>
        <button onClick={() => navigate('/forgot-password')}>Tekrar Dene</button>
      </div>
    );
  }

  if (success) {
    return (
      <div style={{ textAlign: 'center', marginTop: '80px', padding: '20px' }}>
        <h2 style={{ color: 'green' }}>✓ Şifre Başarıyla Güncellendi</h2>
        <p>Giriş sayfasına yönlendiriliyorsunuz...</p>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: '400px', margin: '80px auto', padding: '20px' }}>
      <h2>Yeni Şifre Belirle</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Yeni Şifre</label>
          <input
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
            disabled={loading}
            style={{ display: 'block', width: '100%', marginTop: '8px', padding: '8px' }}
          />
        </div>
        <div style={{ marginTop: '12px' }}>
          <label>Şifre Tekrar</label>
          <input
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            disabled={loading}
            style={{ display: 'block', width: '100%', marginTop: '8px', padding: '8px' }}
          />
        </div>
        {error && <p style={{ color: 'red', marginTop: '10px' }}>{error}</p>}
        <button
          type="submit"
          disabled={loading}
          style={{ marginTop: '16px', padding: '10px 20px' }}
        >
          {loading ? 'Kaydediliyor...' : 'Şifreyi Güncelle'}
        </button>
      </form>
    </div>
  );
};

export default ResetPassword;
