
import { ProgressSpinner } from 'primereact/progressspinner';
import { useState, useEffect } from "react";
import { InputNumber } from 'primereact/inputnumber';
import { Checkbox } from "primereact/checkbox";
import { Divider } from 'primereact/divider';
import { Button } from 'primereact/button';

export default function ImageGenerationConfiguration({ onCallToast }) {
    const [isLoading, setIsLoading] = useState(true);

    const [padding, setPadding] = useState(28);
    const [backgroundOpacity, setBackgroundOpacity] = useState(120);
    const [textAtTop, setTextAtTop] = useState(true);

    const [isSavingConfiguration, setIsSavingConfiguration] = useState(false);

    const apiUrl = '/api/Configuration';

    const getConfiguration = async () => {
        fetch(`${apiUrl}`)
            .then(response => response.json())
            .then(json => {
                console.log(json);
                setIsLoading(false);
                setBackgroundOpacity(json.backgroundOpacity);
                setPadding(json.textPadding);
                setTextAtTop(json.textAtTop);
            })
            .catch(error => {
                console.error('Error fetching configuration:', error);
                setIsLoading(false);
                onCallToast(1, 'Failed to fetch configuration')
            });
    }

    const saveConfiguration = async () => {
        setIsSavingConfiguration(true);
        const response = await fetch(apiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ TextPadding: padding, TextAtTop: textAtTop, BackgroundOpacity: backgroundOpacity })
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
                            <label htmlFor="minmax" className="font-bold block mb-2">Padding</label>
                            <InputNumber inputId="minmax" showButtons value={padding} onValueChange={(e) => setPadding(e.value)} min={10} max={50} />
                        </div>
                        <div className="flex-auto mt-5">
                            <label htmlFor="minmax" className="font-bold block mb-2">Background opacity</label>
                            <InputNumber inputId="minmax" showButtons value={backgroundOpacity} onValueChange={(e) => setBackgroundOpacity(e.value)} min={80} max={210} />
                        </div>
                        <div className="flex-auto mt-5">
                            <label htmlFor="minmax" className="font-bold block mb-2">Draw text at {textAtTop ? ('top') : ('bottom')}</label>
                            <Checkbox onChange={e => setTextAtTop(e.checked)} checked={textAtTop}></Checkbox>
                        </div>
                        <Divider />
                        <Button label="Submit" icon="pi pi-check" loading={isSavingConfiguration} onClick={saveConfiguration} />
                    </div>
                )}
        </>
    );
}