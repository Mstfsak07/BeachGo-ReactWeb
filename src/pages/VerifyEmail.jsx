import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import authService from '../services/authService';

const VerifyEmail = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState('loading'); // 'loading' | 'success' | 'error'
  const [message, setMessage] = useState('');

  useEffect(() => {
    const token = searchParams.get('token');
    if (!token) {
      setStatus('error');
      setMessage('Geçersiz doğrulama bağlantısı. Token bulunamadı.');
      return;
    }

    authService.verifyEmail(token)
      .then(() => {
        setStatus('success');
        setMessage('E-posta adresiniz başarıyla doğrulandı!');
        setTimeout(() => navigate('/login'), 3000);
      })
      .catch((err) => {
        setStatus('error');
        setMessage(
          err?.response?.data?.message ||
          err?.response?.data ||
          'E-posta doğrulama başarısız. Token geçersiz veya süresi dolmuş olabilir.'
        );
      });
  }, []);

  return (
    <div style={{ textAlign: 'center', marginTop: '80px', padding: '20px' }}>
      {status === 'loading' && <p>E-posta doğrulanıyor, lütfen bekleyin...</p>}
      {status === 'success' && (
        <div>
          <h2 style={{ color: 'green' }}>✓ {message}</h2>
          <p>Giriş sayfasına yönlendiriliyorsunuz...</p>
        </div>
      )}
      {status === 'error' && (
        <div>
          <h2 style={{ color: 'red' }}>✗ Doğrulama Başarısız</h2>
          <p>{message}</p>
          <button onClick={() => navigate('/login')}>Giriş Sayfasına Dön</button>
        </div>
      )}
    </div>
  );
};

export default VerifyEmail;
