import React, { useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import authService from '../services/authService';

const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const token = searchParams.get('token');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!token) {
      setError('Geçersiz sıfırlama bağlantısı. Token bulunamadı.');
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
        err?.response?.data?.title ||
        'Şifre sıfırlama başarısız. Token geçersiz veya süresi dolmuş olabilir.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (!token) {
    return (
      <div style={{ textAlign: 'center', padding: '50px', maxWidth: '500px', margin: '0 auto' }}>
        <h2>Şifre Sıfırla</h2>
        <p style={{ color: 'red' }}>Geçersiz sıfırlama bağlantısı. Token bulunamadı.</p>
        <button onClick={() => navigate('/forgot-password')}>
          Tekrar Dene
        </button>
      </div>
    );
  }

  if (success) {
    return (
      <div style={{ textAlign: 'center', padding: '50px', maxWidth: '500px', margin: '0 auto' }}>
        <h2>Şifre Sıfırla</h2>
        <div style={{ color: 'green' }}>
          <p>Şifreniz başarıyla sıfırlandı! Giriş sayfasına yönlendiriliyorsunuz...</p>
        </div>
      </div>
    );
  }

  return (
    <div style={{ textAlign: 'center', padding: '50px', maxWidth: '500px', margin: '0 auto' }}>
      <h2>Şifre Sıfırla</h2>

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        <input
          type="password"
          placeholder="Yeni şifre (en az 6 karakter)"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          required
          disabled={loading}
          style={{ padding: '10px', fontSize: '16px' }}
        />

        <input
          type="password"
          placeholder="Yeni şifre (tekrar)"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          required
          disabled={loading}
          style={{ padding: '10px', fontSize: '16px' }}
        />

        {error && <p style={{ color: 'red' }}>{error}</p>}

        <button type="submit" disabled={loading} style={{ padding: '10px', fontSize: '16px' }}>
          {loading ? 'Sıfırlanıyor...' : 'Şifremi Sıfırla'}
        </button>
      </form>
    </div>
  );
};

export default ResetPassword;