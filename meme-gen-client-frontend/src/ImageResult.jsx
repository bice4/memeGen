import './App.css';
import { Button } from 'primereact/button';

export default function ImageResult({ data, onTryAgain }) {

    return (
        <div className='flex align-items-center justify-content-center h-screen'>
            {data.status === 1 && (
                <div className='flex flex-column'>
                    <img
                        src={`data:image/png;base64,${data.imageBase64}`}
                        alt="Result"
                        style={{
                            maxWidth: "400px",
                            maxHeight: "400px",
                            borderRadius: "12px",
                            boxShadow: "0 8px 20px rgba(0,0,0,0.3)",
                        }}
                    />
                    <Button className='mt-5' label="Try again" severity="secondary" onClick={onTryAgain} text />
                </div>
            )}

            {data.status === 2 && (
                <div className='text-center text-lg'>
                    <div>🤖 Blue Screen of Emotion... Generator overheated, restarting core 🚀</div>
                    <div className='mt-2 text-blue-300'>{data.additionalInformation}</div>
                    <Button className='mt-5' label="Try again" severity="secondary" onClick={onTryAgain} text />

                </div>
            )}

        </div>
    )
}
