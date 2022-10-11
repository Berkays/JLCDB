import React from 'react';

export function Layout(props) {
    return (
        <div className='d-flex flex-column'>
            <div className='container d-flex flex-fill flex-column align-items-center'>
                {props.children}
            </div>
            <div className='container-fluid p-0'>
                <footer>
                    <div className="container-fluid footer-container">
                        <span>Made with <i className="bi bi-heart-fill" style={{ color: '#e80d0d' }} />
                            &nbsp;By&nbsp;
                            <a href="https://github.com/Berkays" target="_blank" rel="noreferrer">Berkay Gürsoy</a>
                            <a className="ps-2" href="https://github.com/Berkays/berkaygursoy" aria-label="GitHub"
                                target="_blank" rel="noreferrer">
                                <title>Source</title>
                                <i className="bi bi-github"></i>
                            </a>
                        </span>
                    </div>
                </footer>
            </div >
        </div >
    );
}
