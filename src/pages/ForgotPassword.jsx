import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '../services/authService';

const ForgotPassword = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await authService.forgotPassword(email);
      setSuccess(true);
    } catch (err) {
      setError(
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        'Bir hata oluştu. Lütfen tekrar deneyin.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div style={{ textAlign: 'center', padding: '50px', maxWidth: '500px', margin: '0 auto' }}>
        <h2>Şifremi Unuttum</h2>
        <div style={{ color: 'green' }}>
          <p>Şifre sıfırlama bağlantısı email adresinize gönderildi.</p>
          <p>Lütfen email kutunuzu kontrol edin.</p>
        </div>
        <button onClick={() => navigate('/login')} style={{ marginTop: '16px' }}>
          Giriş Sayfasına Dön
        </button>
      </div>
    );
  }

  return (
    <div style={{ textAlign: 'center', padding: '50px', maxWidth: '500px', margin: '0 auto' }}>
      <h2>Şifremi Unuttum</h2>
      <p>Email adresinizi girin, şifre sıfırlama bağlantısı göndereceğiz.</p>

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        <input
          type="email"
          placeholder="Email adresiniz"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          disabled={loading}
          style={{ padding: '10px', fontSize: '16px' }}
        />

        {error && <p style={{ color: 'red' }}>{error}</p>}

        <button type="submit" disabled={loading} style={{ padding: '10px', fontSize: '16px' }}>
          {loading ? 'Gönderiliyor...' : 'Sıfırlama Bağlantısı Gönder'}
        </button>

        <button type="button" onClick={() => navigate('/login')} disabled={loading}>
          Giriş Sayfasına Dön
        </button>
      </form>
    </div>
  );
};

export default ForgotPassword;
