
import { TabView, TabPanel } from 'primereact/tabview';
import ImageGenerationConfiguration from './ImageGenerationConfiguration'

export default function Configuration({ onCallToast }) {

    return (
        <TabView>
            <TabPanel header="Image generation" leftIcon="pi pi-images mr-2">
                <div className='flex'>
                    <div className='col-12'>
                        <ImageGenerationConfiguration onCallToast={onCallToast} />
                    </div>
                </div>
            </TabPanel>
        </TabView>
    )
}