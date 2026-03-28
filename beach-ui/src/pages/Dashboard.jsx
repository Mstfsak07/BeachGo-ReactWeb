import React, { useEffect, useState } from "react";
import api from "../api/axios";
import toast from "react-hot-toast";

const Dashboard = () => {
    const [beaches, setBeaches] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchBeaches = async () => {
            try {
                const response = await api.get("/beaches");
                setBeaches(response.data.data);
            } catch (err) {
                console.error(err);
                toast.error("Veriler yüklenirken hata oluştu.");
            } finally {
                setLoading(false);
            }
        };

        fetchBeaches();
    }, []);

    if (loading) return <p>Yükleniyor...</p>;

    return (
        <div className="p-8 pt-24">
            <h1 className="text-3xl font-black mb-8">
                Plajlar Listesi (Korumalı)
            </h1>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {beaches?.map((beach) => (
                    <div
                        key={beach.id}
                        className="p-6 bg-white rounded-2xl shadow-sm border border-slate-100"
                    >
                        <h3 className="text-xl font-bold text-slate-800">
                            {beach.name}
                        </h3>
                        <p className="text-slate-500">{beach.address}</p>
                        <div className="mt-4 text-blue-600 font-bold">
                            {beach.rating} / 5
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default Dashboard;