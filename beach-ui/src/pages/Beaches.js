import React, { useState, useEffect } from 'react';
import { getBeaches, searchBeaches } from '../services/api';
import BeachCard from '../components/BeachCard';
import Loading from '../components/common/Loading';

const Beaches = () => {
  const [beaches, setBeaches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const res = await getBeaches();
      setBeaches(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await searchBeaches(query);
      setBeaches(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen pt-24 pb-20 px-6">
      <div className="container mx-auto">
        {/* Header & Search */}
        <div className="flex flex-col md:flex-row justify-between items-center mb-12 gap-8">
          <div>
            <h1 className="text-5xl font-black text-slate-800 tracking-tighter mb-2">Plajları Keşfet</h1>
            <p className="text-slate-500 font-medium">Antalya'nın en güzel plajlarını kriterlerinize göre bulun.</p>
          </div>

          <form onSubmit={handleSearch} className="w-full md:w-auto flex-grow max-w-xl relative">
            <input
              type="text"
              className="input-field pl-12 pr-4 py-4 text-lg"
              placeholder="Plaj adı veya konum ara..."
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
            <svg 
              xmlns="http://www.w3.org/2000/svg" 
              className="h-6 w-6 absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" 
              fill="none" viewBox="0 0 24 24" stroke="currentColor"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <button type="submit" className="absolute right-2 top-2 bottom-2 btn-primary px-6 text-sm">
              Ara
            </button>
          </form>
        </div>

        {/* Results Grid */}
        {loading ? (
          <Loading />
        ) : (
          <>
            {beaches.length === 0 ? (
              <div className="text-center py-20 card bg-slate-50 border-dashed border-2">
                <div className="text-6xl mb-4">🏖️</div>
                <h3 className="text-2xl font-bold text-slate-700 mb-2">Sonuç Bulunamadı</h3>
                <p className="text-slate-500 mb-6">Aramanızla eşleşen bir plaj bulamadık. Lütfen farklı kelimeler deneyin.</p>
                <button onClick={fetchData} className="btn-secondary">Tüm Plajları Göster</button>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
                {beaches.map((beach) => (
                  <BeachCard key={beach.id} beach={beach} />
                ))}
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default Beaches;
