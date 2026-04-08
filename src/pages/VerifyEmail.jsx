import React, { useState, useEffect } from 'react';
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

    const verifyToken = async () => {
      try {
        await authService.verifyEmail(token);
        setStatus('success');
        setMessage('Email adresiniz başarıyla doğrulandı! Giriş sayfasına yönlendiriliyorsunuz...');
        setTimeout(() => navigate('/login'), 3000);
      } catch (error) {
        setStatus('error');
        setMessage(
          error?.response?.data?.message ||
          error?.response?.data?.title ||
          'Email doğrulama başarısız. Token geçersiz veya süresi dolmuş olabilir.'
        );
      }
    };

    verifyToken();
  }, [searchParams, navigate]);

  return (
    <div style={{ textAlign: 'center', padding: '50px', maxWidth: '500px', margin: '0 auto' }}>
      <h2>Email Doğrulama</h2>

      {status === 'loading' && (
        <div>
          <p>Email adresiniz doğrulanıyor, lütfen bekleyin...</p>
        </div>
      )}

      {status === 'success' && (
        <div style={{ color: 'green' }}>
          <p>{message}</p>
        </div>
      )}

      {status === 'error' && (
        <div style={{ color: 'red' }}>
          <p>{message}</p>
          <button onClick={() => navigate('/login')} style={{ marginTop: '16px' }}>
            Giriş Sayfasına Git
          </button>
        </div>
      )}
    </div>
  );
};

export default VerifyEmail;
