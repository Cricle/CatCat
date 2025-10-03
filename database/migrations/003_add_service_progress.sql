-- Create service_progress table
CREATE TABLE IF NOT EXISTS service_progress (
    id BIGINT PRIMARY KEY,
    order_id BIGINT NOT NULL,
    service_provider_id BIGINT NOT NULL,
    status INTEGER NOT NULL,
    description TEXT,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    address TEXT,
    image_urls TEXT,
    created_at TIMESTAMP NOT NULL,
    FOREIGN KEY (order_id) REFERENCES service_orders(id) ON DELETE CASCADE,
    FOREIGN KEY (service_provider_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS idx_service_progress_order_id ON service_progress(order_id);
CREATE INDEX IF NOT EXISTS idx_service_progress_created_at ON service_progress(created_at DESC);

-- Comments
COMMENT ON TABLE service_progress IS 'Service progress tracking for real-time updates';
COMMENT ON COLUMN service_progress.status IS '1=OnTheWay, 2=Arrived, 3=StartService, 4=Feeding, 5=CleaningLitter, 6=Playing, 7=Grooming, 8=TakingPhotos, 9=Completed';
COMMENT ON COLUMN service_progress.image_urls IS 'JSON array of image URLs for service photos';

