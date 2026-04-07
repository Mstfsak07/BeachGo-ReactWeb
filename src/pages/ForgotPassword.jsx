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
        err?.response?.data ||
        'Bir hata oluştu. Lütfen tekrar deneyin.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div style={{ textAlign: 'center', marginTop: '80px', padding: '20px' }}>
        <h2 style={{ color: 'green' }}>E-posta Gönderildi</h2>
        <p>Şifre sıfırlama bağlantısı e-posta adresinize gönderildi. Lütfen gelen kutunuzu kontrol edin.</p>
        <button onClick={() => navigate('/login')}>Giriş Sayfasına Dön</button>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: '400px', margin: '80px auto', padding: '20px' }}>
      <h2>Şifremi Unuttum</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label>E-posta Adresi</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
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
          {loading ? 'Gönderiliyor...' : 'Sıfırlama Bağlantısı Gönder'}
        </button>
      </form>
      <p style={{ marginTop: '16px' }}>
        <button onClick={() => navigate('/login')} style={{ background: 'none', border: 'none', cursor: 'pointer', textDecoration: 'underline' }}>
          Giriş sayfasına dön
        </button>
      </p>
    </div>
  );
};

export default ForgotPassword;
