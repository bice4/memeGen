
import { ProgressSpinner } from 'primereact/progressspinner';
import { useState, useEffect } from "react";
import { InputNumber } from 'primereact/inputnumber';
import { Divider } from 'primereact/divider';
import { Button } from 'primereact/button';

export default function ImageCachingConfiguration({ onCallToast }) {
    const [isLoading, setIsLoading] = useState(true);

    const [cacheDurationInMinutes, setCacheDurationInMinutes] = useState(60);
    const [imageRetentionInMinutes, setImageRetentionInMinutes] = useState(120);

    const [isSavingConfiguration, setIsSavingConfiguration] = useState(false);

    const apiUrl = '/api/Configuration';

    const getConfiguration = async () => {
        fetch(`${apiUrl}/2`)
            .then(response => response.json())
            .then(json => {
                setIsLoading(false);
                setCacheDurationInMinutes(json.cacheDurationInMinutes);
                setImageRetentionInMinutes(json.imageRetentionInMinutes);
            })
            .catch(error => {
                console.error('Error fetching configuration:', error);
                setIsLoading(false);
                onCallToast(1, 'Failed to fetch configuration')
            });
    }

    const saveConfiguration = async () => {
        setIsSavingConfiguration(true);
        const response = await fetch(`${apiUrl}/imageCaching`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ CacheDurationInMinutes: cacheDurationInMinutes, ImageRetentionInMinutes: imageRetentionInMinutes })
        });

        if (response.ok) {
            onCallToast(0, 'Configuration updated');
            getConfiguration();

        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to update configuration');
        }

        setIsSavingConfiguration(false);
    }

    useEffect(() => {
        getConfiguration();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);


    return (
        <>
            {isLoading ?
                (<ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="8" fill="var(--surface-ground)" animationDuration=".5s" />)
                : (
                    <div>
                        <div className="flex-auto">
                            <label htmlFor="minmax" className="font-bold block mb-2">Cache duration in minutes</label>
                            <InputNumber inputId="minmax" showButtons value={cacheDurationInMinutes} onValueChange={(e) => setCacheDurationInMinutes(e.value)} min={1} max={1440} />
                        </div>
                        <div className="flex-auto mt-5">
                            <label htmlFor="minmax" className="font-bold block mb-2">Image retention in minutes</label>
                            <InputNumber inputId="minmax" showButtons value={imageRetentionInMinutes} onValueChange={(e) => setImageRetentionInMinutes(e.value)} min={2} max={10080} />
                        </div>
                        <Divider />
                        <Button label="Submit" icon="pi pi-check" loading={isSavingConfiguration} onClick={saveConfiguration} />
                    </div>
                )}
        </>
    );
}