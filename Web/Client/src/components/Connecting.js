export function Connecting() {
    return (
        <div className="d-flex flex-column align-items-center my-auto gap-3">
            <div className="spinner-border text-secondary" role="status" style={{ width: '4rem', height: '4rem' }}>
                <span className="visually-hidden">Connecting...</span>
            </div>
            <h1 className="display-4 text-secondary">Connecting...</h1>
        </div >
    );
}