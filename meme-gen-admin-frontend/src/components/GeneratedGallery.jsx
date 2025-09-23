
import { ProgressSpinner } from 'primereact/progressspinner';
import { useState, useEffect } from "react";
import { Galleria } from 'primereact/galleria';


export default function GeneratedGallery({ selectedPerson, onCallToast }) {

    const [isLoading, setIsLoading] = useState(true);
    const [images, setImages] = useState([]);

    const apiUrl = '/api/Template';


    const itemTemplate = (item) => {
        return <img src={item.itemImageSrc} alt={item.alt} style={{ maxWidth: '25rem', display: 'block' }} />;
    };

    const getGeneratedImages = async () => {
        fetch(`${apiUrl}/content/${selectedPerson.id}`)
            .then(response => response.json())
            .then(json => {
                setIsLoading(false);
                setImages(json.map(item => ({
                    itemImageSrc: `data:image/png;base64,${item}`
                })));
            })
            .catch(error => {
                console.error('Error fetching photos:', error);
                setIsLoading(false);
                onCallToast(1, 'Failed to fetch photos')
            });
    }

    useEffect(() => {
        getGeneratedImages();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedPerson]);


    return (
        <div className="flex align-items-center justify-content-center">

            {isLoading ?
                (<ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="8" fill="var(--surface-ground)" animationDuration=".5s" />)
                : images.length === 0
                    ? (<div>No generated images found</div>)
                    : (<Galleria value={images} numVisible={5} style={{ width: 'w-5' }}
                        item={itemTemplate} showThumbnails={false} showIndicators showItemNavigators circular />)}
        </div>
    );
}